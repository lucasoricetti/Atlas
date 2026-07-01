using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class CloudProviderService
{
    private readonly IUnitOfWork _uow;

    public CloudProviderService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<CloudProviderReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.CloudProviders.GetAllAsync(ct);
        return list.Select(cp => cp.ToDto()).ToList();
    }

    public async Task<CloudProviderReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.CloudProviders.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<CloudProviderReadDto> CreateAsync(CloudProviderCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.CloudProviders.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<CloudProviderReadDto> UpdateAsync(string id, CloudProviderCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.CloudProviders.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"CloudProvider con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<CloudProviderReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.CloudProviders.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"CloudProvider con id '{id}' non trovato.");

        await _uow.CloudProviders.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.CloudProviders.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un CloudProvider con Name='{name}'.");
    }
}
