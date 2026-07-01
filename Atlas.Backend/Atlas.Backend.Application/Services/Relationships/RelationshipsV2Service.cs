using Atlas.Backend.Application.DTOs.Relationships;
using Atlas.Backend.Application.IRepositories.Relationships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Application.Services;

/// <summary>
/// Servizio applicativo per la gestione delle relazioni v2.
/// </summary>
public class RelationshipsV2Service
{
    private const int MaxBatchOperations = 500;
    private const string AddOperation = "add";
    private const string RemoveOperation = "remove";
    private const string UpdateOperation = "update";

    private readonly IRelationshipsV2Repository _repository;

    public RelationshipsV2Service(IRelationshipsV2Repository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Restituisce le relazioni attive per una combinazione source/relation.
    /// </summary>
    public async Task<IReadOnlyList<RelationshipV2ListItemDto>> GetActiveRelationsAsync(
        string sourceType,
        string sourceId,
        string relationType,
        CancellationToken ct = default)
    {
        var definition = ResolveDefinition(sourceType, relationType);
        var list = await _repository.GetActiveRelationsAsync(definition, sourceId, ct);
        return list.Select(ToDto).ToList();
    }

    /// <summary>
    /// Restituisce i candidati collegabili con paginazione a cursore.
    /// </summary>
    public async Task<RelationshipV2CandidatesResponseDto> GetCandidatesAsync(
        string sourceType,
        string sourceId,
        string relationType,
        string? search,
        string? cursor,
        int limit,
        string sortBy,
        string sortDir,
        bool excludeLinked,
        bool includeTotal,
        CancellationToken ct = default)
    {
        ValidateCandidateQuery(limit, sortBy, sortDir);

        var definition = ResolveDefinition(sourceType, relationType);
        var page = await _repository.GetCandidatesAsync(
            definition,
            sourceId,
            search,
            cursor,
            limit,
            sortDir,
            excludeLinked,
            includeTotal,
            ct);

        return new RelationshipV2CandidatesResponseDto
        {
            Items = page.Items.Select(x => new RelationshipV2CandidateItemDto
            {
                TargetId = x.TargetId,
                TargetLabel = x.TargetLabel,
                Metadata = x.Metadata
            }).ToList(),
            NextCursor = page.NextCursor,
            TotalApprox = page.TotalApprox
        };
    }

    /// <summary>
    /// Crea una nuova relazione.
    /// </summary>
    public async Task<RelationshipV2ListItemDto> AddAsync(
        string sourceType,
        string sourceId,
        string relationType,
        RelationshipV2AddRequestDto request,
        CancellationToken ct = default)
    {
        var definition = ResolveDefinition(sourceType, relationType);

        if (!definition.SupportsCritical && request.IsCritical is not null)
        {
            throw new ArgumentException("La relazione selezionata non supporta il campo IsCritical.");
        }

        var item = await _repository.AddAsync(definition, sourceId, request.TargetId, request.IsCritical, ct);
        return ToDto(item);
    }

    /// <summary>
    /// Aggiorna una relazione esistente.
    /// </summary>
    public async Task<RelationshipV2ListItemDto> UpdateAsync(
        string sourceType,
        string sourceId,
        string relationType,
        string relationId,
        RelationshipV2UpdateRequestDto request,
        CancellationToken ct = default)
    {
        var definition = ResolveDefinition(sourceType, relationType);

        if (!definition.SupportsCritical)
        {
            throw new ArgumentException("La relazione selezionata non supporta l'aggiornamento IsCritical.");
        }

        var item = await _repository.UpdateAsync(definition, sourceId, relationId, request.IsCritical, ct);
        return ToDto(item);
    }

    /// <summary>
    /// Rimuove una relazione.
    /// </summary>
    public Task<bool> RemoveAsync(
        string sourceType,
        string sourceId,
        string relationType,
        string relationId,
        CancellationToken ct = default)
    {
        var definition = ResolveDefinition(sourceType, relationType);
        return _repository.RemoveAsync(definition, sourceId, relationId, ct);
    }

    /// <summary>
    /// Esegue un batch di operazioni add/remove/update.
    /// </summary>
    public async Task<RelationshipV2BatchResponseDto> ExecuteBatchAsync(
        string sourceType,
        string sourceId,
        string relationType,
        RelationshipV2BatchRequestDto request,
        CancellationToken ct = default)
    {
        var definition = ResolveDefinition(sourceType, relationType);
        ValidateBatchRequest(request);

        var operations = request.Operations
            .Select(op => ToBatchOperation(definition, op))
            .ToList();

        var result = await _repository.ExecuteBatchAsync(definition, sourceId, operations, ct);

        return new RelationshipV2BatchResponseDto
        {
            Summary = new RelationshipV2BatchSummaryDto
            {
                Added = result.Summary.Added,
                Removed = result.Summary.Removed,
                Updated = result.Summary.Updated,
                Skipped = result.Summary.Skipped
            },
            CurrentRelations = result.CurrentRelations.Select(ToDto).ToList()
        };
    }

    private RelationshipV2Definition ResolveDefinition(string sourceType, string relationType)
    {
        if (!_repository.TryResolveDefinition(sourceType, relationType, out var definition))
        {
            throw new ArgumentException(
                $"Combinazione sourceType/relationType non supportata: '{sourceType}/{relationType}'.");
        }

        return definition;
    }

    private static void ValidateCandidateQuery(int limit, string sortBy, string sortDir)
    {
        if (!string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Il parametro sortBy supporta solo il valore 'name'.");
        }

        if (!string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Il parametro sortDir supporta solo 'asc' o 'desc'.");
        }

        if (limit is < 1 or > 200)
        {
            throw new ArgumentException("Il parametro limit deve essere compreso tra 1 e 200.");
        }
    }

    private static void ValidateBatchRequest(RelationshipV2BatchRequestDto request)
    {
        if (request.Operations.Count == 0)
        {
            throw new ArgumentException("Il batch deve contenere almeno un'operazione.");
        }

        if (request.Operations.Count > MaxBatchOperations)
        {
            throw new ArgumentException($"Il batch non può superare {MaxBatchOperations} operazioni.");
        }
    }

    private static RelationshipV2BatchOperation ToBatchOperation(
        RelationshipV2Definition definition,
        RelationshipV2BatchOperationDto operation)
    {
        var normalizedOperation = (operation.Op ?? string.Empty).Trim().ToLowerInvariant();
        ValidateOperationType(normalizedOperation, operation.Op);
        ValidateOperationPayload(definition, normalizedOperation, operation);

        return new RelationshipV2BatchOperation(
            normalizedOperation,
            operation.TargetId,
            operation.RelationId,
            operation.IsCritical);
    }

    private static void ValidateOperationType(string normalizedOperation, string originalOperation)
    {
        if (normalizedOperation is AddOperation or RemoveOperation or UpdateOperation)
        {
            return;
        }

        throw new ArgumentException($"Operazione non supportata: '{originalOperation}'.");
    }

    private static void ValidateOperationPayload(
        RelationshipV2Definition definition,
        string normalizedOperation,
        RelationshipV2BatchOperationDto operation)
    {
        if (normalizedOperation == AddOperation && string.IsNullOrWhiteSpace(operation.TargetId))
        {
            throw new ArgumentException("L'operazione 'add' richiede TargetId.");
        }

        if ((normalizedOperation == RemoveOperation || normalizedOperation == UpdateOperation)
            && string.IsNullOrWhiteSpace(operation.RelationId))
        {
            throw new ArgumentException($"L'operazione '{normalizedOperation}' richiede RelationId.");
        }

        if (normalizedOperation == UpdateOperation && operation.IsCritical is null)
        {
            throw new ArgumentException("L'operazione 'update' richiede IsCritical.");
        }

        if (!definition.SupportsCritical && operation.IsCritical is not null)
        {
            throw new ArgumentException("IsCritical non è supportato per il relationType selezionato.");
        }
    }

    private static RelationshipV2ListItemDto ToDto(RelationshipV2Item item)
    {
        return new RelationshipV2ListItemDto
        {
            RelationId = item.RelationId,
            TargetId = item.TargetId,
            TargetLabel = item.TargetLabel,
            IsCritical = item.IsCritical
        };
    }
}
