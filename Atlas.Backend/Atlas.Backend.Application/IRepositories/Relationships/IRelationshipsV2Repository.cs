using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Application.IRepositories.Relationships;

/// <summary>
/// Definizione statica di una combinazione sourceType/relationType.
/// </summary>
public sealed record RelationshipV2Definition(
    string SourceType,
    string RelationType,
    string SourceLabel,
    string TargetLabel,
    string TargetLabelProperty,
    string Neo4jRelationType,
    bool SupportsCritical,
    bool ReverseDirection = false);

/// <summary>
/// Relazione attiva restituita dalle API v2.
/// </summary>
public sealed record RelationshipV2Item(
    string RelationId,
    string TargetId,
    string TargetLabel,
    bool? IsCritical);

/// <summary>
/// Candidato collegabile ad una relazione v2.
/// </summary>
public sealed record RelationshipV2CandidateItem(
    string TargetId,
    string TargetLabel,
    Dictionary<string, object?>? Metadata = null);

/// <summary>
/// Pagina di candidati con cursor pagination.
/// </summary>
public sealed record RelationshipV2CandidatesPage(
    IReadOnlyList<RelationshipV2CandidateItem> Items,
    string? NextCursor,
    long? TotalApprox);

/// <summary>
/// Operazione batch elementare sulle relazioni v2.
/// </summary>
public sealed record RelationshipV2BatchOperation(
    string Op,
    string? TargetId,
    string? RelationId,
    bool? IsCritical);

/// <summary>
/// Totali operazioni batch elaborate.
/// </summary>
public sealed record RelationshipV2BatchSummary(int Added, int Removed, int Updated, int Skipped);

/// <summary>
/// Esito batch con riepilogo e stato corrente delle relazioni.
/// </summary>
public sealed record RelationshipV2BatchResult(
    RelationshipV2BatchSummary Summary,
    IReadOnlyList<RelationshipV2Item> CurrentRelations);

/// <summary>
/// Contratto repository per la gestione relazioni v2.
/// </summary>
public interface IRelationshipsV2Repository
{
    /// <summary>
    /// Risolve la definizione della combinazione sourceType/relationType.
    /// </summary>
    bool TryResolveDefinition(string sourceType, string relationType, out RelationshipV2Definition definition);

    /// <summary>
    /// Restituisce le relazioni attive del nodo sorgente.
    /// </summary>
    Task<IReadOnlyList<RelationshipV2Item>> GetActiveRelationsAsync(
        RelationshipV2Definition definition,
        string sourceId,
        CancellationToken ct = default);

    /// <summary>
    /// Restituisce i candidati collegabili, con filtro, ordinamento e paginazione.
    /// </summary>
    Task<RelationshipV2CandidatesPage> GetCandidatesAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string? search,
        string? cursor,
        int limit,
        string sortDir,
        bool excludeLinked,
        bool includeTotal,
        CancellationToken ct = default);

    /// <summary>
    /// Crea una nuova relazione.
    /// </summary>
    Task<RelationshipV2Item> AddAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string targetId,
        bool? isCritical,
        CancellationToken ct = default);

    /// <summary>
    /// Aggiorna una relazione esistente.
    /// </summary>
    Task<RelationshipV2Item> UpdateAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string relationId,
        bool? isCritical,
        CancellationToken ct = default);

    /// <summary>
    /// Rimuove una relazione esistente.
    /// </summary>
    Task<bool> RemoveAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string relationId,
        CancellationToken ct = default);

    /// <summary>
    /// Esegue un batch di operazioni atomiche sulle relazioni.
    /// </summary>
    Task<RelationshipV2BatchResult> ExecuteBatchAsync(
        RelationshipV2Definition definition,
        string sourceId,
        IReadOnlyList<RelationshipV2BatchOperation> operations,
        CancellationToken ct = default);
}
