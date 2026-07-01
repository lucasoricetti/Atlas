using Atlas.Backend.Application.Definitions;
using Atlas.Backend.Application.IRepositories.Relationships;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

/// <summary>
/// Neo4j implementation of the relationship graph repository.
///
/// This class is responsible for:
/// - validating subject types and filters
/// - executing Cypher queries
/// - aggregating nodes and edges in a de-duplicated graph result
/// </summary>
public class RelationshipGraphRepository : IRelationshipGraphRepository
{
    /// <summary>
    /// Internal description of a node schema,
    /// mapping a logical subject type to a Neo4j label.
    /// </summary>
    private sealed record NodeSchema(string Label, string Type);

    /// <summary>
    /// Raw edge row returned by Cypher queries.
    /// This shape is intentionally flat to simplify mapping.
    /// </summary>
    private sealed record EdgeRow(
        string SourceId,
        string SourceType,
        string SourceLabel,
        string TargetId,
        string TargetType,
        string TargetLabel,
        string RelationType,
        bool? IsCritical);

    /// <summary>
    /// Central mapping between logical subject types
    /// and Neo4j labels.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, NodeSchema> SubjectSchemas =
        new Dictionary<string, NodeSchema>(StringComparer.OrdinalIgnoreCase)
        {
            ["asset"] = new("Asset", "asset"),
            ["service"] = new("Service", "service"),
            ["division"] = new("Division", "division"),
            ["contract"] = new("Contract", "contract"),
            ["supplier"] = new("Supplier", "supplier"),
            ["cloudprovider"] = new("CloudProvider", "cloudprovider"),
            ["virtualmachine"] = new("VirtualMachine", "virtualmachine"),
            ["setting"] = new("Setting", "setting"),
            ["logintype"] = new("LoginType", "logintype"),
            ["process"] = new("Process", "process"),
            ["acnmacroarea"] = new("AcnMacroArea", "acnmacroarea")
        };

    private readonly IDriver _driver;
    private readonly string _db;

    public RelationshipGraphRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public IReadOnlyCollection<string> SupportedSubjectTypes => SubjectSchemas.Keys.ToList();

    public IReadOnlyCollection<string> SupportedIncludes => GraphFilterRegistry.GetAll().Keys.ToList();

    public IReadOnlyList<GraphFilterDefinition> GetAvailableFilters(string subjectType)
    {
        return GraphFilterRegistry.GetForSubjectType(subjectType);
    }

    public async Task<RelationshipGraphResult> QueryAsync(
        RelationshipGraphSubject subject,
        IReadOnlyCollection<string> includes,
        int dependencyDepth,
        CancellationToken ct = default)
    {
        // Resolve schema for the requested subject type
        if (!SubjectSchemas.TryGetValue(subject.Type, out var subjectSchema))
        {
            throw new ArgumentException($"Unsupported SubjectType: '{subject.Type}'.");
        }

        // Validate all includes before touching the database
        ValidateIncludesForSubject(includes, subject.Type);

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));

        // Resolve and validate the root subject node
        var subjectNode = await GetSubjectNodeAsync(session, subjectSchema, subject.Id);
        if (subjectNode is null)
        {
            throw new KeyNotFoundException(
                $"Node of type '{subject.Type}' with id '{subject.Id}' not found.");
        }

        // Use dictionaries to prevent duplicated nodes / edges
        var nodeMap = new Dictionary<string, RelationshipGraphNode>(StringComparer.OrdinalIgnoreCase)
        {
            [NodeKey(subjectNode.Type, subjectNode.Id)] = subjectNode
        };

        var edgeMap = new Dictionary<string, RelationshipGraphEdge>(StringComparer.OrdinalIgnoreCase);

        // Execute each filter independently and merge results
        foreach (var include in includes)
        {
            if (!GraphFilterRegistry.TryGet(include, out var filter))
            {
                throw new ArgumentException($"Unsupported include: '{include}'.");
            }

            // Each filter generates its own Cypher query
            var cypher = filter!.QueryFactory(subjectSchema.Label, dependencyDepth);

            var result = await session.RunAsync(cypher, new { subjectId = subject.Id });
            var rows = await result.ToListAsync(MapRow);

            foreach (var row in rows)
            {
                // Nodes are de-duplicated using a composite key
                nodeMap.TryAdd(
                    NodeKey(row.SourceType, row.SourceId),
                    new RelationshipGraphNode(row.SourceId, row.SourceType, row.SourceLabel));

                nodeMap.TryAdd(
                    NodeKey(row.TargetType, row.TargetId),
                    new RelationshipGraphNode(row.TargetId, row.TargetType, row.TargetLabel));

                // Edges are also de-duplicated to avoid overlaps
                edgeMap.TryAdd(
                    EdgeKey(row),
                    new RelationshipGraphEdge(
                        row.SourceId,
                        row.SourceType,
                        row.TargetId,
                        row.TargetType,
                        row.RelationType,
                        row.IsCritical));
            }
        }

        return new RelationshipGraphResult(
            nodeMap.Values.ToList(),
            edgeMap.Values.ToList());
    }

    /// <summary>
    /// Ensures all requested filters are supported by the subject type.
    /// </summary>
    private static void ValidateIncludesForSubject(IEnumerable<string> includes, string subjectType)
    {
        foreach (var include in includes)
        {
            if (!GraphFilterRegistry.IsSupported(include, subjectType))
            {
                throw new ArgumentException(
                    $"Filter '{include}' is not supported for SubjectType '{subjectType}'.");
            }
        }
    }

    /// <summary>
    /// Maps a Neo4j record to an internal EdgeRow.
    /// </summary>
    private static EdgeRow MapRow(IRecord record)
    {
        return new EdgeRow(
            record["sourceId"].As<string>(),
            record["sourceType"].As<string>(),
            record["sourceLabel"].As<string>(),
            record["targetId"].As<string>(),
            record["targetType"].As<string>(),
            record["targetLabel"].As<string>(),
            record["relationType"].As<string>(),
            record["isCritical"].As<bool?>());
    }

    /// <summary>
    /// Resolves the root subject node and basic display data.
    /// </summary>
    private async Task<RelationshipGraphNode?> GetSubjectNodeAsync(
        IAsyncSession session,
        NodeSchema schema,
        string subjectId)
    {
        var query = $@"
            MATCH (subject:{schema.Label} {{Id: $subjectId}})
            RETURN
             subject.Id AS id,
             toLower(head(labels(subject))) AS type,
             coalesce(subject.Name, subject.Id) AS label";

        var result = await session.RunAsync(query, new { subjectId });
        var record = await result.SingleOrDefaultAsync();

        return record is null
            ? null
            : new RelationshipGraphNode(
                record["id"].As<string>(),
                record["type"].As<string>(),
                record["label"].As<string>());
    }

    /// <summary>
    /// Generates a unique key for a graph node.
    /// </summary>
    private static string NodeKey(string type, string id)
        => $"{type}:{id}";

    /// <summary>
    /// Generates a unique key for a graph edge.
    ///
    /// IsCritical is part of the key to avoid collapsing
    /// semantically distinct edges.
    /// </summary>
    private static string EdgeKey(EdgeRow row)
        => $"{row.SourceType}:{row.SourceId}|{row.RelationType}|{row.TargetType}:{row.TargetId}|{row.IsCritical}";
}
