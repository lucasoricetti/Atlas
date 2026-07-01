using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services
{
    public class AcnMacroAreaService
    {
        private readonly IUnitOfWork _uow;

        public AcnMacroAreaService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<AcnMacroAreaReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _uow.AcnMacroAreas.GetAllAsync(ct);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<AcnMacroAreaReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.AcnMacroAreas.GetByIdAsync(id, ct);
            return entity?.ToDto();
        }

        public async Task<AcnMacroAreaReadDto> CreateAsync(AcnMacroAreaCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

            var entity = dto.ToEntity(Guid.NewGuid().ToString());
            await _uow.AcnMacroAreas.CreateAsync(entity, ct);

            return entity.ToDto();
        }

        public async Task<AcnMacroAreaReadDto> UpdateAsync(string id, AcnMacroAreaCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

            var entity = dto.ToEntity(id);
            var updated = await _uow.AcnMacroAreas.UpdateAsync(entity, ct);
            if (!updated)
                throw new KeyNotFoundException($"AcnMacroArea con id '{id}' non trovato.");

            return entity.ToDto();
        }

        public async Task<AcnMacroAreaReadDto> DeleteAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.AcnMacroAreas.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"AcnMacroArea con id '{id}' non trovato.");

            await _uow.AcnMacroAreas.DeleteAsync(id, ct);
            return entity.ToDto();
        }

        private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = await _uow.AcnMacroAreas.GetByNameAsync(name, ct);
            if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
                throw new DuplicateNameException($"Esiste già un AcnMacroArea con Name='{name}'.");
        }
    }
}
