using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.Mappers;
using Atlas.Backend.Application.UnitOfWork;
using System.Data;

namespace Atlas.Backend.Application.Services
{
    public class ServiceService
    {
        private readonly IUnitOfWork _uow;

        public ServiceService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<ServiceReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _uow.Services.GetAllAsync(ct);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<ServiceReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Services.GetByIdAsync(id, ct);
            return entity?.ToDto();
        }

        public async Task<ServiceReadDto> CreateAsync(ServiceCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, null, ct);

            var entity = dto.ToEntity(Guid.NewGuid().ToString());
            await _uow.Services.CreateAsync(entity, ct);

            return entity.ToDto();
        }

        public async Task<ServiceReadDto> UpdateAsync(string id, ServiceCreateDto dto, CancellationToken ct = default)
        {
            await EnsureUniqueNameOrThrowAsync(dto.Name, id, ct);

            var entity = dto.ToEntity(id);
            var updated = await _uow.Services.UpdateAsync(entity, ct);
            if (!updated)
                throw new KeyNotFoundException($"Service con id '{id}' non trovato.");

            return entity.ToDto();
        }

        public async Task<ServiceReadDto> DeleteAsync(string id, CancellationToken ct = default)
        {
            var entity = await _uow.Services.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Service con id '{id}' non trovato.");

            await _uow.Services.DeleteAsync(id, ct);
            return entity.ToDto();
        }

        private async Task EnsureUniqueNameOrThrowAsync(string? name, string? currentId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = await _uow.Services.GetByNameAsync(name, ct);
            if (existing is not null && !string.Equals(existing.Id, currentId, StringComparison.Ordinal))
                throw new DuplicateNameException($"Esiste già un Service con Name='{name}'.");
        }
    }
}
