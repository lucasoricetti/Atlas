using Atlas.Backend.Core.Entities;
using Atlas.Backend.Application.DTOs.Entities;

namespace Atlas.Backend.Application.Mappers
{
    public static class AssetMapper
    {
        public static AssetReadDto ToDto(this Asset entity)
        {
            return new AssetReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                Description = entity.Description,
                Criticality = entity.Criticality,
                Bia = entity.Bia,
                RpoH = entity.RpoH,
                MtoH = entity.MtoH
            };
        }

        public static Asset ToEntity(this AssetCreateDto dto, string id)
            => MapToEntity(dto, id);

        public static Asset MapToEntity(AssetCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new Asset
            {
                Id = id,
                Name = dto.Name!,
                Type = dto.Type!.Value,
                Description = dto.Description,
                Criticality = dto.Criticality!.Value,
                Bia = dto.Bia!.Value,
                RpoH = dto.RpoH,
                MtoH = dto.MtoH
            };
        }
    }
}
