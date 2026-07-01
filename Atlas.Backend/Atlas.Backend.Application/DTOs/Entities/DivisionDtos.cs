using System;
using System.Collections.Generic;
using FluentValidation;
using System.Text;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class DivisionCreateDto
    {
        public string? Name { get; set; }
    }

    public class DivisionCreateDtoValidator : AbstractValidator<DivisionCreateDto>
    {
        public DivisionCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");
        }
    }

    public class DivisionReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}
