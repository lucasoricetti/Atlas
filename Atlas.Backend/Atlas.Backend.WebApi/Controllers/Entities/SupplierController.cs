using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Backend.WebApi.Controllers.Entities;

[ApiController]
[Route("api/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly SupplierService _service;

    public SupplierController(SupplierService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id}")]
    [Authorize(Policy = "ReaderPolicy")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = $"Supplier con id '{id}' non trovato." });

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Create([FromBody] SupplierCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Update(string id, [FromBody] SupplierCreateDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
