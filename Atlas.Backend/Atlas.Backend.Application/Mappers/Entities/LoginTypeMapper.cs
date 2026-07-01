using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers;

public static class LoginTypeMapper
{
    public static LoginTypeReadDto ToDto(this LoginType entity)
    {
        return new LoginTypeReadDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Mfa = entity.Mfa,
            Protocol = entity.Protocol
        };
    }

    public static LoginType ToEntity(this LoginTypeCreateDto dto, string id)
        => MapToEntity(dto, id);

    public static LoginType MapToEntity(LoginTypeCreateDto dto, string id)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new LoginType
        {
            Id = id,
            Name = dto.Name!,
            Mfa = dto.Mfa!.Value,
            Protocol = dto.Protocol
        };
    }
}
