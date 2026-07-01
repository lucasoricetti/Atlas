using Atlas.Backend.Core.Enums;
using FluentValidation;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class AssetCreateDto
    {
        public string? Name { get; set; }
        public AssetType? Type { get; set; }
        public string? Description { get; set; }
        public Criticality? Criticality { get; set; }
        public bool? Bia { get; set; }
        public int? RpoH { get; set; }
        public int? MtoH { get; set; }
    }

    public class AssetCreateDtoValidator : AbstractValidator<AssetCreateDto>
    {
        public AssetCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");

            RuleFor(x => x.Type)
                .NotNull().WithMessage("The Type field is required.");

            RuleFor(x => x.Criticality)
                .NotNull().WithMessage("The Criticality field is required.");

            RuleFor(x => x.Bia)
                .NotNull().WithMessage("The Bia field is required.");
        }
    }

    public class AssetReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required AssetType Type { get; set; }
        public string? Description { get; set; }
        public required Criticality Criticality { get; set; }
        public required bool Bia { get; set; }
        public int? RpoH { get; set; }
        public int? MtoH { get; set; }
    }
}