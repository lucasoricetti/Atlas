using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers
{
    public static class ProcessMapper
    {
        public static ProcessReadDto ToDto(this Process entity)
        {
            return new ProcessReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public static Process ToEntity(this ProcessCreateDto dto, string id)
            => MapToEntity(dto, id);

        public static Process MapToEntity(ProcessCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new Process
            {
                Id = id,
                Name = dto.Name!,
                Description = dto.Description
            };
        }
    }
}
