using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers;

public static class SettingMapper
{
    public static SettingReadDto ToDto(this Setting entity)
    {
        return new SettingReadDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Links = entity.Links,
            Description = entity.Description
        };
    }

    public static Setting ToEntity(this SettingCreateDto dto, string id)
        => MapToEntity(dto, id);

    public static Setting MapToEntity(SettingCreateDto dto, string id)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new Setting
        {
            Id = id,
            Name = dto.Name!,
            Links = dto.Links ?? [],
            Description = dto.Description
        };
    }
}