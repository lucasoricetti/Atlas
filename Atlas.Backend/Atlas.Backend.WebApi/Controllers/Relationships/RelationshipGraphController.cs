using Atlas.Backend.Application.DTOs.Relationships;
using Atlas.Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.WebApi.Controllers;

/// <summary>
/// REST API controller responsible for exposing
/// capabilities and query operations for the relationship graph.
///
/// This controller is intentionally thin:
/// all validation and business logic are delegated
/// to the RelationshipGraphService.
/// </summary>
[ApiController]
[Route("api/graph")]
public class RelationshipGraphController : ControllerBase
{
    private readonly RelationshipGraphService _service;

    /// <summary>
    /// Initializes the controller with the graph application service.
    /// </summary>
    /// <param name="service">
    /// Application service handling graph capabilities and queries.
    /// </param>
    public RelationshipGraphController(RelationshipGraphService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns the generic graph capabilities.
    ///
    /// This endpoint exists mainly for backward compatibility and
    /// provides only the raw lists of subject types and includes.
    /// </summary>
    [HttpGet("capabilities")]
    [Authorize(Policy = "ReaderPolicy")]
    public IActionResult GetCapabilities()
    {
        return Ok(_service.GetCapabilities());
    }

    /// <summary>
    /// Returns the available filters (capabilities) for a specific subject type.
    ///
    /// The response is enriched with UI metadata such as:
    /// - user-friendly labels
    /// - descriptions
    /// - category grouping
    /// - display order
    /// </summary>
    /// <param name="subjectType">
    /// Logical type of the subject node (e.g. "asset", "service").
    /// </param>
    [HttpGet("capabilities/{subjectType}")]
    [Authorize(Policy = "ReaderPolicy")]
    public IActionResult GetCapabilitiesBySubjectType(string subjectType)
    {
        return Ok(_service.GetCapabilitiesBySubjectType(subjectType));
    }

    /// <summary>
    /// Executes a relationship graph query starting from a given subject.
    ///
    /// The query may include multiple filters and optionally expand
    /// dependencies up to a configurable depth.
    /// </summary>
    /// <param name="request">
    /// Query definition including subject, filters and dependency depth.
    /// </param>
    /// <param name="ct">
    /// Cancellation token propagated to Neo4j queries.
    /// </param>
    [HttpPost("query")]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> Query(
        [FromBody] RelationshipGraphQueryRequestDto request,
        CancellationToken ct)
    {
        var graph = await _service.QueryAsync(request, ct);
        return Ok(graph);
    }
}