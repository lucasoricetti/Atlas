using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Atlas.Backend.Application.Services;

public class ContractService
{
    private readonly IUnitOfWork _uow;

    public ContractService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ContractReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _uow.Contracts.GetAllAsync(ct);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<ContractReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<ContractReadDto> CreateAsync(ContractCreateDto dto, CancellationToken ct = default)
    {
        Normalize(dto);
        await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

        var entity = dto.ToEntity(Guid.NewGuid().ToString());
        await _uow.Contracts.CreateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<ContractReadDto> UpdateAsync(string id, ContractCreateDto dto, CancellationToken ct = default)
    {
        Normalize(dto);
        await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

        var entity = dto.ToEntity(id);
        var updated = await _uow.Contracts.UpdateAsync(entity, ct);
        if (!updated)
            throw new KeyNotFoundException($"Contract con id '{id}' non trovato.");

        return entity.ToDto();
    }

    public async Task<ContractReadDto> DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _uow.Contracts.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Contract con id '{id}' non trovato.");

        await _uow.Contracts.DeleteAsync(id, ct);
        return entity.ToDto();
    }

    private static void Normalize(ContractCreateDto dto)
    {
        dto.Name = dto.Name?.Trim();
        dto.ContactEmail = string.IsNullOrWhiteSpace(dto.ContactEmail) ? null : dto.ContactEmail.Trim();
        dto.ContactPhone = string.IsNullOrWhiteSpace(dto.ContactPhone) ? null : dto.ContactPhone.Trim();
        dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
    }

    private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _uow.Contracts.GetByNameAsync(name, ct);
        if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
            throw new DuplicateNameException($"Esiste già un Contract con Name='{name}'.");
    }
}