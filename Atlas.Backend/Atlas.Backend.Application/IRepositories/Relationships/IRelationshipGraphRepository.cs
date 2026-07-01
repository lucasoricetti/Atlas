using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Application.IRepositories.Relationships;

/// <summary>
/// Identifica il nodo sorgente della query grafo.
/// </summary>
public sealed record RelationshipGraphSubject(string Type, string Id);

/// <summary>
/// Rappresenta un nodo del grafo relazionale.
/// </summary>
public sealed record RelationshipGraphNode(string Id, string Type, string Label);

/// <summary>
/// Rappresenta un arco direzionato del grafo relazionale.
/// </summary>
public sealed record RelationshipGraphEdge(
    string SourceId,
    string SourceType,
    string TargetId,
    string TargetType,
    string RelationType,
    bool? IsCritical);

/// <summary>
/// Contiene il risultato completo della query grafo.
/// </summary>
public sealed record RelationshipGraphResult(
    IReadOnlyList<RelationshipGraphNode> Nodes,
    IReadOnlyList<RelationshipGraphEdge> Edges);

/// <summary>
/// Contratto repository per interrogare il grafo relazionale.
/// </summary>

/// <summary>
/// Repository contract for querying the relationship graph.
///
/// This abstraction hides the underlying graph engine (Neo4j)
/// and exposes a domain-oriented API to the application layer.
/// </summary>
public interface IRelationshipGraphRepository
{
    /// <summary>
    /// List of subject types supported by the graph.
    ///
    /// Example values: "asset", "service", "division".
    /// These values are expected to be normalized (lowercase).
    /// </summary>
    IReadOnlyCollection<string> SupportedSubjectTypes { get; }

    /// <summary>
    /// List of all supported include identifiers.
    ///
    /// This is mainly exposed for backward compatibility
    /// and does not reflect per-subject filtering rules.
    /// </summary>
    IReadOnlyCollection<string> SupportedIncludes { get; }

    /// <summary>
    /// Returns the available graph filters for a specific subject type.
    ///
    /// The result is intended to be consumed by the UI
    /// and is already ordered for display purposes.
    /// </summary>
    /// <param name="subjectType">
    /// Normalized subject type (lowercase).
    /// </param>
    IReadOnlyList<GraphFilterDefinition> GetAvailableFilters(string subjectType);

    /// <summary>
    /// Executes a graph query starting from a subject node.
    /// </summary>
    /// <param name="subject">
    /// Graph subject (type + identifier) representing the query root.
    /// </param>
    /// <param name="includes">
    /// List of filter identifiers defining which relationship
    /// expansions must be applied.
    /// </param>
    /// <param name="dependencyDepth">
    /// Maximum depth for recursive dependency traversal.
    /// Used only by filters that explicitly require it.
    /// </param>
    /// <param name="ct">
    /// Cancellation token propagated to the database driver.
    /// </param>
    Task<RelationshipGraphResult> QueryAsync(
        RelationshipGraphSubject subject,
        IReadOnlyCollection<string> includes,
        int dependencyDepth,
        CancellationToken ct = default);
}