using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.WebApi.Controllers.Entities;

[ApiController]
[Route("api/contracts")]
public class ContractController : ControllerBase
{
    private readonly ContractService _service;
    public ContractController(ContractService service) => _service = service;

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
            return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = $"Contract con id '{id}' non trovato." });

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Create([FromBody] ContractCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Update(string id, [FromBody] ContractCreateDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}