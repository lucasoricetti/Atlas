using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class LoginTypeService
{
    private readonly IUnitOfWork _uow;

    public LoginTypeService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<LoginTypeReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.LoginTypes.GetAllAsync(ct);
        return list.Select(lt => lt.ToDto()).ToList();
    }

    public async Task<LoginTypeReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.LoginTypes.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<LoginTypeReadDto> CreateAsync(LoginTypeCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.LoginTypes.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<LoginTypeReadDto> UpdateAsync(string id, LoginTypeCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.LoginTypes.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"LoginType con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<LoginTypeReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.LoginTypes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"LoginType con id '{id}' non trovato.");

        await _uow.LoginTypes.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.LoginTypes.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un LoginType con Name='{name}'.");
    }
}
