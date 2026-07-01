using Atlas.Backend.Core.Entities;
using Atlas.Backend.Application.DTOs.Entities;

namespace Atlas.Backend.Application.Mappers
{
    public static class DivisionMapper
    {
        public static DivisionReadDto ToDto(this Division entity)
        {
            return new DivisionReadDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public static Division ToEntity(this DivisionCreateDto dto, string id)
            => MapToEntity(dto, id);

        public static Division MapToEntity(DivisionCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new Division
            {
                Id = id,
                Name = dto.Name!
            };
        }
    }
}
