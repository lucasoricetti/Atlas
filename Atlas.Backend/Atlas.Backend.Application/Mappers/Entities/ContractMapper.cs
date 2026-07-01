using Atlas.Backend.Core.Entities;
using Atlas.Backend.Application.DTOs.Entities;
using System;
using System.Linq;

namespace Atlas.Backend.Application.Mappers
{
    public static class ContractMapper
    {
        public static ContractReadDto ToDto(this Contract entity)
        {
            return new ContractReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ContractTypes = entity.ContractTypes?.ToList() ?? new(),
                Sla = entity.Sla,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Notes = entity.Notes
            };
        }

        public static Contract ToEntity(this ContractCreateDto dto, string id)
            => MapToEntity(dto, id);

        private static Contract MapToEntity(ContractCreateDto dto, string id)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return new Contract
            {
                Id = id,
                Name = dto.Name!,
                ContractTypes = dto.ContractTypes?.ToList() ?? new(),
                Sla = dto.Sla,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Notes = dto.Notes
            };
        }
    }
}