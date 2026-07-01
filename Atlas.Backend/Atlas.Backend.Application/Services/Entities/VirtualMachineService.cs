using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services;

public class VirtualMachineService
{
    private readonly IUnitOfWork _uow;

    public VirtualMachineService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<VirtualMachineReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.VirtualMachines.GetAllAsync(ct);
        return list.Select(vm => vm.ToDto()).ToList();
    }

    public async Task<VirtualMachineReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.VirtualMachines.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<VirtualMachineReadDto> CreateAsync(VirtualMachineCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.VirtualMachines.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<VirtualMachineReadDto> UpdateAsync(string id, VirtualMachineCreateDto dto, CancellationToken ct = default)
    {
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.VirtualMachines.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"VirtualMachine con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<VirtualMachineReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.VirtualMachines.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"VirtualMachine con id '{id}' non trovato.");

        await _uow.VirtualMachines.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.VirtualMachines.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già una VirtualMachine con Name='{name}'.");
    }
}
