using Atlas.Backend.Application.DTOs.Relationships;
using Atlas.Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.WebApi.Controllers;

/// <summary>
/// Endpoint API v2 per consultazione e gestione relazioni.
/// </summary>
[ApiController]
[Route("api/v2/{sourceType}/{sourceId}/{relationType}")]
public class RelationshipsV2Controller : ControllerBase
{
    private readonly RelationshipsV2Service _service;

    public RelationshipsV2Controller(RelationshipsV2Service service)
    {
        _service = service;
    }

    /// <summary>
    /// Restituisce le relazioni attive per un determinato soggetto.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetActiveRelations(
        string sourceType,
        string sourceId,
        string relationType,
        CancellationToken ct)
    {
        var items = await _service.GetActiveRelationsAsync(sourceType, sourceId, relationType, ct);
        return Ok(items);
    }

    /// <summary>
    /// Restituisce i candidati collegabili con filtro e paginazione.
    /// </summary>
    [HttpGet("candidates")]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetCandidates(
        string sourceType,
        string sourceId,
        string relationType,
        [FromQuery] string? search,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 25,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDir = "asc",
        [FromQuery] bool excludeLinked = true,
        [FromQuery] bool includeTotal = false,
        CancellationToken ct = default)
    {
        var page = await _service.GetCandidatesAsync(
            sourceType,
            sourceId,
            relationType,
            search,
            cursor,
            limit,
            sortBy,
            sortDir,
            excludeLinked,
            includeTotal,
            ct);

        return Ok(page);
    }

    /// <summary>
    /// Crea una nuova relazione.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Add(
        string sourceType,
        string sourceId,
        string relationType,
        [FromBody] RelationshipV2AddRequestDto request,
        CancellationToken ct)
    {
        var created = await _service.AddAsync(sourceType, sourceId, relationType, request, ct);
        return Created($"/api/v2/{sourceType}/{sourceId}/{relationType}/{created.RelationId}", created);
    }

    /// <summary>
    /// Aggiorna una relazione esistente.
    /// </summary>
    [HttpPatch("{relationId}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Update(
        string sourceType,
        string sourceId,
        string relationType,
        string relationId,
        [FromBody] RelationshipV2UpdateRequestDto request,
        CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(sourceType, sourceId, relationType, relationId, request, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Rimuove una relazione esistente.
    /// </summary>
    [HttpDelete("{relationId}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Remove(
        string sourceType,
        string sourceId,
        string relationType,
        string relationId,
        CancellationToken ct)
    {
        var removed = await _service.RemoveAsync(sourceType, sourceId, relationType, relationId, ct);

        if (!removed)
        {
            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = "Relazione non trovata."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Esegue operazioni batch su una relazione.
    /// </summary>
    [HttpPost("batch")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Batch(
        string sourceType,
        string sourceId,
        string relationType,
        [FromBody] RelationshipV2BatchRequestDto request,
        CancellationToken ct)
    {
        var result = await _service.ExecuteBatchAsync(sourceType, sourceId, relationType, request, ct);
        return Ok(result);
    }
}
