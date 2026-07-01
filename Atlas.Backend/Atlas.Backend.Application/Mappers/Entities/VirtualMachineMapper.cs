using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers;

public static class VirtualMachineMapper
{
    public static VirtualMachineReadDto ToDto(this VirtualMachine entity)
    {
        return new VirtualMachineReadDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Ip = entity.Ip,
            Cluster = entity.Cluster,
            Role = entity.Role
        };
    }

    public static VirtualMachine ToEntity(this VirtualMachineCreateDto dto, string id)
        => MapToEntity(dto, id);

    public static VirtualMachine MapToEntity(VirtualMachineCreateDto dto, string id)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new VirtualMachine
        {
            Id = id,
            Name = dto.Name!,
            Type = dto.Type!.Value,
            Ip = dto.Ip,
            Cluster = dto.Cluster,
            Role = dto.Role
        };
    }
}
