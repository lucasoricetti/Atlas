using Atlas.Backend.Core.Enums;
using System;
using System.Collections.Generic;
using FluentValidation;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class ContractCreateDto
    {
        public string? Name { get; set; }

        public List<ContractType> ContractTypes { get; set; } = new();

        public int? Sla { get; set; }

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ContractCreateDtoValidator : AbstractValidator<ContractCreateDto>
    {
        public ContractCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");

            RuleFor(x => x.ContractTypes)
                .NotEmpty().WithMessage("The ContractTypes field is required.");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
                .WithMessage("The email address is invalid.");

            RuleFor(x => x.ContactPhone)
                .Matches(@"^\+?[0-9\s-]+$").When(x => !string.IsNullOrEmpty(x.ContactPhone))
                .WithMessage("The phone number is invalid.");
        }
    }

    public class ContractReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<ContractType> ContractTypes { get; set; } = new();
        public int? Sla { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}