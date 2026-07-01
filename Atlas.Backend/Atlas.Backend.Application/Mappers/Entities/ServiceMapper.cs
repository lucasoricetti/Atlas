using Atlas.Backend.Core.Entities;
using Atlas.Backend.Application.DTOs.Entities;

namespace Atlas.Backend.Application.Mappers
{
    public static class ServiceMapper
    {
        public static ServiceReadDto ToDto(this Service entity)
        {
            return new ServiceReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Category = entity.Category,
                Version = entity.Version,
                ProtocolPort = entity.ProtocolPort,
                Env = entity.Env,
                Status = entity.Status,
                Description = entity.Description
            };
        }

        public static Service ToEntity(this ServiceCreateDto dto, string id)
            => MapToEntity(dto, id);

        public static Service MapToEntity(ServiceCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new Service
            {
                Id = id,
                Name = dto.Name!,
                Category = dto.Category,
                Version = dto.Version,
                ProtocolPort = dto.ProtocolPort,
                Env = dto.Env!.Value,
                Status = dto.Status,
                Description = dto.Description
            };
        }
    }
}
