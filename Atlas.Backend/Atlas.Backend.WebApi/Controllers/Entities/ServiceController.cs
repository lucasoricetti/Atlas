using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Services;
using Atlas.Backend.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Backend.WebApi.Controllers;

[ApiController]
[Route("api/services")]
public class ServiceController : ControllerBase
{
    private readonly ServiceService _service;
    public ServiceController(ServiceService service) => _service = service;

    // GET api/services
    [HttpGet]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    // GET api/services/{id}
    [HttpGet("{id}")]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = $"Service con id '{id}' non trovato." });

        return Ok(item);
    }

    // POST api/services
    [HttpPost]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/services/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Update(string id, [FromBody] ServiceCreateDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, dto, ct));

    // DELETE api/services/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
