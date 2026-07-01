using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Core.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class ProcessCreateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class ProcessCreateDtoValidator : AbstractValidator<ProcessCreateDto>
    {
        public ProcessCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");
        }
    }
    public class  ProcessReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
