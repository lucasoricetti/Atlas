using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.Mappers;

public static class SupplierMapper
{
    public static SupplierReadDto ToDto(this Supplier entity)
    {
        return new SupplierReadDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public static Supplier ToEntity(this SupplierCreateDto dto, string id)
        => MapToEntity(dto, id);

    private static Supplier MapToEntity(SupplierCreateDto dto, string id)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new Supplier
        {
            Id = id,
            Name = dto.Name!
        };
    }
}
