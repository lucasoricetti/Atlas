using Atlas.Backend.Core.Enums;
using FluentValidation;
using System.Text.Json.Serialization;

namespace Atlas.Backend.Application.DTOs.Entities
{
    public class AcnMacroAreaCreateDto
    {
        public string? Name { get; set; }
        public AcnCategoryOfRelevance? PreAssignedAcnCategory { get; set; }
        public AcnCategoryOfRelevance? CustomAcnCategory { get; set; }
    }

    public class AcnMacroAreaCreateDtoValidator : AbstractValidator<AcnMacroAreaCreateDto>
    {
        public AcnMacroAreaCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.");

            RuleFor(x => x.PreAssignedAcnCategory)
                .NotNull().WithMessage("The PreAssignedAcnCategory field is required.");
        }
    }

    public class AcnMacroAreaReadDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required AcnCategoryOfRelevance PreAssignedAcnCategory { get; set; }
        public AcnCategoryOfRelevance? CustomAcnCategory { get; set; }
    }
}
