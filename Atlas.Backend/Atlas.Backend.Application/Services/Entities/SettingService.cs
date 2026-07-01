using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class SettingService
{
    private readonly IUnitOfWork _uow;

    public SettingService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SettingReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.Settings.GetAllAsync(ct);
        return list.Select(s => s.ToDto()).ToList();
    }

    public async Task<SettingReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Settings.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<SettingReadDto> CreateAsync(SettingCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.Settings.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<SettingReadDto> UpdateAsync(string id, SettingCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.Settings.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"Setting con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<SettingReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Settings.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Setting con id '{id}' non trovato.");

        await _uow.Settings.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.Settings.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un Setting con Name='{name}'.");
    }
}
