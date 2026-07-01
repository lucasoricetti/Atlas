using Atlas.Backend.Core.Enums;
using FluentValidation;
using System.Text.Json.Serialization;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class ServiceCreateDto
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Version { get; set; }
        public string? ProtocolPort { get; set; }

        public Env? Env { get; set; }

        public Status? Status { get; set; }
        public string? Description { get; set; }
    }

    public class ServiceCreateDtoValidator : AbstractValidator<ServiceCreateDto>
    {
        public ServiceCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");

            RuleFor(x => x.Env)
                .NotNull().WithMessage("The Env field is required.");
        }
    }

    public class ServiceReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Category { get; set; }
        public string? Version { get; set; }
        public string? ProtocolPort { get; set; }
        public required Env Env { get; set; }
        public Status? Status { get; set; }
        public string? Description { get; set; }
    }
}
