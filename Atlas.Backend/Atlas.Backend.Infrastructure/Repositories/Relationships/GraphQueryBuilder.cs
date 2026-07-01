using System.Text;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

/// <summary>
/// Describes the direction of a graph step relative to the current node.
/// </summary>
internal enum StepDirection
{
    /// <summary>
    /// Traverses from the current node to a node reached through an outgoing relationship:
    /// current -[:REL]-> next
    /// </summary>
    Outgoing,

    /// <summary>
    /// Traverses from a node into the current node through an incoming relationship:
    /// previous -[:REL]-> current
    /// </summary>
    Incoming
}

/// <summary>
/// Describes the node reached after a graph step.
/// </summary>
/// <param name="Label">
/// Optional Neo4j label constraint for the target node.
/// When set, the rendered pattern becomes something like <c>(n:Label)</c>.
/// </param>
/// <param name="Alias">
/// Optional Cypher alias assigned to the node.
/// This makes subsequent steps easier to read and reference.
/// </param>
internal sealed record NodeSpec(
    string? Label = null,
    string? Alias = null);

/// <summary>
/// Represents one traversal step in a Cypher path.
/// A path is rendered as a chain of nodes and relationships, for example:
///
/// <c>(subject)-[:DEPENDS_ON*1..depth]->(n1)-[:HAS_SETTING]->(target)</c>
/// </summary>
/// <param name="RelationshipType">
/// Neo4j relationship type used for this step.
/// </param>
/// <param name="Direction">
/// Whether the step is outgoing or incoming relative to the current node.
/// </param>
/// <param name="Node">
/// Optional node specification for the node reached by this step.
/// </param>
/// <param name="Recursive">
/// When true, the relationship is rendered as a variable-length path:
/// <c>[:REL*min..max]</c>.
/// </param>
/// <param name="MinDepth">
/// Lower bound for recursive traversals.
/// Typical values are 1 for standard recursion and 0 when the first hop is allowed to be optional.
/// </param>
/// <param name="UseDepthMinusOneAsUpperBound">
/// When true, the upper bound becomes <c>depth - 1</c>.
/// This is useful when the recursive segment should stop one hop earlier
/// because a terminal relationship is added afterward.
/// </param>
internal sealed record GraphStep(
    string RelationshipType,
    StepDirection Direction,
    NodeSpec? Node = null,
    bool Recursive = false,
    int MinDepth = 1,
    bool UseDepthMinusOneAsUpperBound = false);

/// <summary>
/// Represents a full path to be rendered as a Cypher MATCH pattern.
/// The path starts from a fixed alias and then expands through one or more steps.
///
/// Visual form:
/// <code>
/// (subject)-[:R1]->(a)-[:R2*1..depth]->(b)
/// </code>
/// </summary>
/// <param name="StartAlias">
/// Alias of the starting node for the path.
/// In this builder the main path usually starts from <c>subject</c>.
/// </param>
/// <param name="Steps">
/// Ordered list of traversal steps.
/// </param>
/// <param name="PathAlias">
/// Alias used for the entire Cypher path, e.g. <c>p</c>, <c>p2</c>.
/// This is important because the builder later unwinds the relationships
/// of one or more named paths.
/// </param>
internal sealed record GraphPath(
    string StartAlias,
    IReadOnlyList<GraphStep> Steps,
    string PathAlias = "p");

/// <summary>
/// Describes a complete graph query shape.
/// </summary>
/// <remarks>
/// The query is built around a main path and optional extra paths.
/// This is useful when the result is not a single chain, but a main traversal
/// plus one or more branch traversals starting from intermediate nodes.
///
/// Visual model:
/// <code>
/// Main path:   subject -[:DEPENDS_ON*]- service
/// Extra path:              service -[:HAS_CONTRACT]-> contract -[:PROVIDED_BY]-> supplier
/// </code>
/// </remarks>
/// <param name="SubjectLabel">
/// Default label of the subject node.
/// This may be overridden at runtime.
/// </param>
/// <param name="MainPath">
/// Primary traversal path.
/// </param>
/// <param name="ExtraPaths">
/// Optional additional traversal paths, rendered as separate MATCH clauses.
/// These are typically used for branch expansions.
/// </param>
/// <param name="ExcludeSubjectNodeFromResults">
/// When true, the builder filters out edges where the subject node appears
/// as source or target in the final result set.
/// This is useful for recursive traversals that would otherwise echo the subject.
/// </param>
internal sealed record GraphQuerySpec(
    string SubjectLabel,
    GraphPath MainPath,
    IReadOnlyList<GraphPath>? ExtraPaths = null,
    bool ExcludeSubjectNodeFromResults = false);

/// <summary>
/// Builds Cypher queries for the graph explorer.
/// </summary>
/// <remarks>
/// The builder does not execute queries. It only converts a declarative graph
/// description into a Cypher string.
///
/// Core idea:
/// instead of hardcoding one method per graph shape, define the shape as data:
/// nodes, directions, recursion, and branch paths.
/// </remarks>
internal static class GraphQueryBuilder
{
    /// <summary>
    /// Builds a query factory from a declarative graph specification.
    /// </summary>
    /// <remarks>
    /// This is the central method of the builder.
    ///
    /// It resolves the subject label at runtime, renders the main path,
    /// optionally renders extra branch paths, unwinds all relationships,
    /// and finally projects a normalized edge shape.
    ///
    /// Visual execution flow:
    /// <code>
    /// MATCH main path
    /// MATCH extra paths
    /// UNWIND relationships(...)
    /// RETURN source / target / relation metadata
    /// </code>
    ///
    /// The returned function keeps the same runtime contract used by the older
    /// query builder: optional label override + recursion depth.
    /// </remarks>
    public static Func<string?, int, string> Build(GraphQuerySpec spec)
        => (labelOverride, depth) =>
        {
            var resolvedSubjectLabel =
                string.IsNullOrWhiteSpace(labelOverride)
                    ? spec.SubjectLabel
                    : labelOverride!;

            var sb = new StringBuilder();

            sb.AppendLine(RenderMainMatch(spec.MainPath, resolvedSubjectLabel, depth));

            if (spec.ExtraPaths is not null)
            {
                foreach (var extraPath in spec.ExtraPaths)
                {
                    sb.AppendLine(RenderExtraMatch(extraPath, depth));
                }
            }

            sb.AppendLine(RenderUnwind(spec));
            sb.AppendLine("WITH DISTINCT");
            sb.AppendLine("startNode(r) AS source,");
            sb.AppendLine("endNode(r) AS target,");
            sb.AppendLine("type(r) AS relationType,");
            sb.AppendLine("r.IsCritical AS isCritical");

            if (spec.ExcludeSubjectNodeFromResults)
            {
                sb.AppendLine("WHERE source.Id <> $subjectId AND target.Id <> $subjectId");
            }

            sb.AppendLine("RETURN");
            sb.AppendLine("source.Id AS sourceId,");
            sb.AppendLine("toLower(head(labels(source))) AS sourceType,");
            sb.AppendLine("coalesce(source.Name, source.Id) AS sourceLabel,");
            sb.AppendLine("target.Id AS targetId,");
            sb.AppendLine("toLower(head(labels(target))) AS targetType,");
            sb.AppendLine("coalesce(target.Name, target.Id) AS targetLabel,");
            sb.AppendLine("relationType,");
            sb.AppendLine("isCritical");

            return sb.ToString();
        };

    /// <summary>
    /// Convenience helper for a single outgoing edge from the subject.
    /// </summary>
    /// <remarks>
    /// Visual form:
    /// <code>
    /// (subject)-[:RELATION]->(target)
    /// </code>
    ///
    /// Use this for direct relationships where the target can optionally be constrained
    /// to a label.
    /// </remarks>
    public static Func<string?, int, string> DirectOutgoing(
        string subjectLabel,
        string relationshipType,
        string? targetLabel = null)
        => Build(new GraphQuerySpec(
            SubjectLabel: subjectLabel,
            MainPath: new GraphPath(
                StartAlias: "subject",
                Steps: new[]
                {
                    new GraphStep(
                        RelationshipType: relationshipType,
                        Direction: StepDirection.Outgoing,
                        Node: new NodeSpec(Label: targetLabel, Alias: "target"))
                })));

    /// <summary>
    /// Convenience helper for a single incoming edge into the subject.
    /// </summary>
    /// <remarks>
    /// Visual form:
    /// <code>
    /// (source)-[:RELATION]->(subject)
    /// </code>
    ///
    /// Use this when the subject is the target of the relationship.
    /// </remarks>
    public static Func<string?, int, string> DirectIncoming(
        string subjectLabel,
        string relationshipType,
        string? sourceLabel = null)
        => Build(new GraphQuerySpec(
            SubjectLabel: subjectLabel,
            MainPath: new GraphPath(
                StartAlias: "subject",
                Steps: new[]
                {
                    new GraphStep(
                        RelationshipType: relationshipType,
                        Direction: StepDirection.Incoming,
                        Node: new NodeSpec(Label: sourceLabel, Alias: "source"))
                })));

    /// <summary>
    /// Renders a linear path made of one or more consecutive steps.
    /// </summary>
    /// <remarks>
    /// Visual form:
    /// <code>
    /// (subject)-[:R1]->(n1)-[:R2]->(n2)-[:R3*1..depth]->(n3)
    /// </code>
    ///
    /// This is the most flexible helper for chain-shaped traversals.
    /// It is ideal when the query can be described as a simple route from
    /// the subject to a final node through ordered hops.
    /// </remarks>
    public static Func<string?, int, string> LinearPath(
        string subjectLabel,
        params GraphStep[] steps)
        => Build(new GraphQuerySpec(
            SubjectLabel: subjectLabel,
            MainPath: new GraphPath("subject", steps)));

    /// <summary>
    /// Renders a main path plus one or more branch paths.
    /// </summary>
    /// <remarks>
    /// Visual form:
    /// <code>
    /// Main:
    ///   (subject)-[:DEPENDS_ON*]->(service)
    ///
    /// Branches:
    ///   (service)-[:HAS_CONTRACT]->(contract)-[:PROVIDED_BY]->(supplier)
    ///   (service)-[:HOSTS]->(vm)
    ///   (service)-[:HOSTS]->(cloudProvider)
    /// </code>
    ///
    /// Use this when a single backbone path fans out into multiple terminal
    /// lookups. This is especially useful for queries that should return
    /// several edge families from the same traversal anchor.
    /// </remarks>
    public static Func<string?, int, string> MainPathWithBranches(
        string subjectLabel,
        GraphPath mainPath,
        params GraphPath[] branches)
        => Build(new GraphQuerySpec(
            SubjectLabel: subjectLabel,
            MainPath: mainPath,
            ExtraPaths: branches));

    /// <summary>
    /// Renders the main MATCH clause for the subject path.
    /// </summary>
    /// <remarks>
    /// This method emits the core pattern starting from the subject node and
    /// expanding along each configured step in order.
    ///
    /// Visual form:
    /// <code>
    /// MATCH p=(subject:Label {Id: $subjectId})-[:REL1]->(n1)-[:REL2*1..depth]->(n2)
    /// </code>
    /// </remarks>
    private static string RenderMainMatch(GraphPath path, string resolvedSubjectLabel, int depth)
    {
        var sb = new StringBuilder();
        sb.Append("MATCH ");
        sb.Append($"{path.PathAlias}=(");
        sb.Append($"subject:{resolvedSubjectLabel} {{Id: $subjectId}}");
        sb.Append(")");

        var currentAlias = "subject";
        var autoIndex = 0;

        foreach (var step in path.Steps)
        {
            autoIndex++;
            var nextAlias = step.Node?.Alias ?? $"n{autoIndex}";
            sb.Append(RenderStep(currentAlias, nextAlias, step, depth));
            currentAlias = nextAlias;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Renders a secondary MATCH clause that starts from a non-subject anchor.
    /// </summary>
    /// <remarks>
    /// This is used for extra branch paths.
    ///
    /// Visual form:
    /// <code>
    /// MATCH p2=(service)-[:HOSTS]->(vm)
    /// </code>
    ///
    /// Unlike the main path, this method does not force the starting node to
    /// be the subject. It simply continues from the provided starting alias.
    /// </remarks>
    private static string RenderExtraMatch(GraphPath path, int depth)
    {
        var sb = new StringBuilder();

        sb.Append("OPTIONAL MATCH ");

        if (!string.IsNullOrWhiteSpace(path.PathAlias))
        {
            sb.Append(path.PathAlias);
            sb.Append("=");
        }

        sb.Append($"({path.StartAlias})");

        var currentAlias = path.StartAlias;
        var autoIndex = 0;

        foreach (var step in path.Steps)
        {
            autoIndex++;
            var nextAlias = step.Node?.Alias ?? $"{path.PathAlias}n{autoIndex}";
            sb.Append(RenderStep(currentAlias, nextAlias, step, depth));
            currentAlias = nextAlias;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Renders one graph step as a Cypher relationship segment.
    /// </summary>
    /// <remarks>
    /// This is the visual heart of the builder.
    ///
    /// Outgoing example:
    /// <code>
    /// -[:REL]->(target:Label)
    /// </code>
    ///
    /// Incoming example:
    /// <code>
    /// <-[:REL]-(source:Label)
    /// </code>
    ///
    /// Recursive example:
    /// <code>
    /// -[:REL*1..depth]->(target)
    /// -[:REL*0..depth-1]->(asset)
    /// </code>
    ///
    /// The method supports both fixed hops and variable-length traversals.
    /// When recursion is enabled, the upper bound is derived from the runtime
    /// depth parameter.
    /// </remarks>
    private static string RenderStep(string fromAlias, string toAlias, GraphStep step, int depth)
    {
        var labelClause = string.IsNullOrWhiteSpace(step.Node?.Label)
            ? string.Empty
            : $":{step.Node!.Label}";

        var upperBound = step.Recursive ? (step.UseDepthMinusOneAsUpperBound ? Math.Max(depth - 1, 0).ToString() : depth.ToString()) : string.Empty;

        var relationshipClause = step.Recursive
            ? $":{step.RelationshipType}*{step.MinDepth}..{upperBound}"
            : $":{step.RelationshipType}";

        return step.Direction == StepDirection.Outgoing
            ? $"-[{relationshipClause}]->({toAlias}{labelClause})"
            : $"<-[{relationshipClause}]-({toAlias}{labelClause})";
    }

    /// <summary>
    /// Renders the UNWIND clause that flattens one or more paths into edges.
    /// </summary>
    /// <remarks>
    /// Visual intent:
    /// <code>
    /// UNWIND relationships(p) AS r
    /// UNWIND relationships(p) + relationships(p2) + relationships(p3) AS r
    /// </code>
    ///
    /// The builder works at edge level, not just path level, so this step
    /// converts complete paths into a stream of individual relationships.
    /// </remarks>
    private static string RenderUnwind(GraphQuerySpec spec)
    {
        var pathAliases = new List<string>();

        if (!string.IsNullOrWhiteSpace(spec.MainPath.PathAlias))
            pathAliases.Add(spec.MainPath.PathAlias);

        if (spec.ExtraPaths is not null)
        {
            pathAliases.AddRange(
                spec.ExtraPaths
                    .Select(x => x.PathAlias)
                    .Where(x => !string.IsNullOrWhiteSpace(x))!);
        }

        if (pathAliases.Count == 0)
            return "UNWIND [] AS r";

        if (pathAliases.Count == 1)
            return $"UNWIND coalesce(relationships({pathAliases[0]}), []) AS r";

        var relLists = string.Join(", ",
            pathAliases.Select(p => $"coalesce(relationships({p}), [])"));

        return
            $@"WITH [{relLists}] AS relCollections
            UNWIND relCollections AS rels
            UNWIND rels AS r";
    }
}