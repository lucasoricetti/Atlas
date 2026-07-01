using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Backend.WebApi.Controllers;

[ApiController]
[Route("api/login-types")]
public class LoginTypeController : ControllerBase
{
    private readonly LoginTypeService _service;
    
    public LoginTypeController(LoginTypeService service) => _service = service;

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
            return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = $"LoginType con id '{id}' non trovato." });

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Create([FromBody] LoginTypeCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Update(string id, [FromBody] LoginTypeCreateDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorPolicy")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
