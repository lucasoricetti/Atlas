using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers;

public static class CloudProviderMapper
{
    public static CloudProviderReadDto ToDto(this CloudProvider entity)
    {
        return new CloudProviderReadDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            PortalUrl = entity.PortalUrl,
            Account = entity.Account
        };
    }

    public static CloudProvider ToEntity(this CloudProviderCreateDto dto, string id)
        => MapToEntity(dto, id);

    public static CloudProvider MapToEntity(CloudProviderCreateDto dto, string id)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new CloudProvider
        {
            Id = id,
            Name = dto.Name!,
            Type = dto.Type!.Value,
            PortalUrl = dto.PortalUrl,
            Account = dto.Account
        };
    }
}
