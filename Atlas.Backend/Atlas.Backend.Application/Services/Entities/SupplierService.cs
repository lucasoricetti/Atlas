using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class SupplierService
{
    private readonly IUnitOfWork _uow;

    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SupplierReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.Suppliers.GetAllAsync(ct);
        return list.Select(s => s.ToDto()).ToList();
    }

    public async Task<SupplierReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<SupplierReadDto> CreateAsync(SupplierCreateDto dto, CancellationToken ct = default)
    {
        Normalize(dto);
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.Suppliers.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<SupplierReadDto> UpdateAsync(string id, SupplierCreateDto dto, CancellationToken ct = default)
    {
        Normalize(dto);
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.Suppliers.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"Supplier con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<SupplierReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Supplier con id '{id}' non trovato.");

        await _uow.Suppliers.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private static void Normalize(SupplierCreateDto dto)
    {
        dto.Name = dto.Name?.Trim();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.Suppliers.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un Supplier con Name='{name}'.");
    }
}
