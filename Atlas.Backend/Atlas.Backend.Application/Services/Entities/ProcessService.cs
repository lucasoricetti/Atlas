using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services
{
    public class ProcessService
    {
        private readonly IUnitOfWork _uow;

        public ProcessService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<ProcessReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _uow.Processes.GetAllAsync(ct);
            return list.Select(p => p.ToDto()).ToList();
        }

        public async Task<ProcessReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Processes.GetByIdAsync(id, ct);
            return entity?.ToDto();
        }

        // Restituisce il DTO completo del Process creato
        public async Task<ProcessReadDto> CreateAsync(ProcessCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

            var entity = dto.ToEntity(Guid.NewGuid().ToString());
            await _uow.Processes.CreateAsync(entity, ct);
            return entity.ToDto();
        }

        // Restituisce il DTO completo del Process aggiornato
        public async Task<ProcessReadDto> UpdateAsync(string id, ProcessCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

            var entity = dto.ToEntity(id);
            var updated = await _uow.Processes.UpdateAsync(entity, ct);
            if (!updated)
                throw new KeyNotFoundException($"Process con id '{id}' non trovato.");

            return entity.ToDto();
        }

        // Recupera l'oggetto prima di eliminarlo e lo restituisce
        public async Task<ProcessReadDto> DeleteAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Processes.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Process con id '{id}' non trovato.");

            await _uow.Processes.DeleteAsync(id, ct);
            return entity.ToDto();
        }

        private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = await _uow.Processes.GetByNameAsync(name, ct);
            if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
                throw new DuplicateNameException($"Esiste già un Process con Name='{name}'.");
        }
    }
}
