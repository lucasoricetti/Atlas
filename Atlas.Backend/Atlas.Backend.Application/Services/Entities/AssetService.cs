using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class AssetService
{
    private readonly IUnitOfWork _uow;

    public AssetService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<AssetReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.Assets.GetAllAsync(ct);
        return list.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssetReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Assets.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    // Restituisce il DTO completo dell'asset creato
    public async Task<AssetReadDto> CreateAsync(AssetCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.Assets.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    // Restituisce il DTO completo dell'asset aggiornato
    public async Task<AssetReadDto> UpdateAsync(string id, AssetCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.Assets.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"Asset con id '{id}' non trovato.");

        return entity.ToDto();
    }

    // Recupera l'oggetto prima di eliminarlo e lo restituisce
    public async Task<AssetReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Assets.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Asset con id '{id}' non trovato.");

        await _uow.Assets.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.Assets.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un Asset con Name='{name}'.");
    }
}
