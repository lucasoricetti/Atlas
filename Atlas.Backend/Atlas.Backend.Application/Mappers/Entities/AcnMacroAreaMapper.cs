using Atlas.Backend.Core.Entities;
using Atlas.Backend.Application.DTOs.Entities;

namespace Atlas.Backend.Application.Mappers
{
    public static class AcnMacroAreaMapper
    {
        public static AcnMacroAreaReadDto ToDto(this AcnMacroArea entity)
        {
            return new AcnMacroAreaReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                PreAssignedAcnCategory = entity.PreAssignedAcnCategory,
                CustomAcnCategory = entity.CustomAcnCategory
            };
        }

        public static AcnMacroArea ToEntity(this AcnMacroAreaCreateDto dto, string id)
            => MapToEntity(dto, id);

        public static AcnMacroArea MapToEntity(AcnMacroAreaCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new AcnMacroArea
            {
                Id = id,
                Name = dto.Name!,
                PreAssignedAcnCategory = dto.PreAssignedAcnCategory!.Value,
                CustomAcnCategory = dto.CustomAcnCategory
            };
        }
    }
}
