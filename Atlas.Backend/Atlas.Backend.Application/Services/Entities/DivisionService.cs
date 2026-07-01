using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services
{
    public class DivisionService
    {
        private readonly IUnitOfWork _uow;

        public DivisionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<DivisionReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _uow.Divisions.GetAllAsync(ct);
            return list.Select(d => d.ToDto()).ToList();
        }

        public async Task<DivisionReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Divisions.GetByIdAsync(id, ct);
            return entity?.ToDto();
        }

        public async Task<DivisionReadDto> CreateAsync(DivisionCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

            var entity = dto.ToEntity(Guid.NewGuid().ToString());
            await _uow.Divisions.CreateAsync(entity, ct);
            return entity.ToDto();
        }

        public async Task<DivisionReadDto> UpdateAsync(string id, DivisionCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

            var entity = dto.ToEntity(id);
            var updated = await _uow.Divisions.UpdateAsync(entity, ct);
            if (!updated)
                throw new KeyNotFoundException($"Division con id '{id}' non trovato.");

            return entity.ToDto();
        }

        public async Task<DivisionReadDto> DeleteAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Divisions.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Division con id '{id}' non trovato.");

            await _uow.Divisions.DeleteAsync(id, ct);
            return entity.ToDto();
        }

        private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = await _uow.Divisions.GetByNameAsync(name, ct);
            if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
                throw new DuplicateNameException($"Esiste già una Division con Name='{name}'.");
        }
    }
}
