using FluentValidation;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Atlas.Backend.Application.DTOs.Relationships;

/// <summary>
/// Richiesta query del grafo relazionale.
/// </summary>
public sealed class RelationshipGraphQueryRequestDto
{
    public string SubjectType { get; set; } = null!;
    public string SubjectId { get; set; } = null!;
    public List<string> Includes { get; set; } = new();
    public int DependencyDepth { get; set; } = 3;
}

public sealed class RelationshipGraphQueryRequestDtoValidator : AbstractValidator<RelationshipGraphQueryRequestDto>
{
    public RelationshipGraphQueryRequestDtoValidator()
    {
        RuleFor(x => x.SubjectType)
            .NotEmpty().WithMessage("The SubjectType field is required.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("The SubjectId field is required.");

        RuleFor(x => x.Includes)
            .NotEmpty().WithMessage("The Includes field must contain at least one relationship.");

        RuleForEach(x => x.Includes)
            .NotEmpty().WithMessage("Includes items cannot be empty.");

        RuleFor(x => x.DependencyDepth)
            .InclusiveBetween(1, 10)
            .WithMessage("The DependencyDepth field must be between 1 and 10.");
    }
}

/// <summary>
/// Nodo restituito nella risposta del grafo.
/// </summary>
public sealed class RelationshipGraphNodeDto
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Label { get; set; }
}

/// <summary>
/// Arco restituito nella risposta del grafo.
/// </summary>
public sealed class RelationshipGraphEdgeDto
{
    public required string SourceId { get; set; }
    public required string SourceType { get; set; }
    public required string TargetId { get; set; }
    public required string TargetType { get; set; }
    public required string RelationType { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCritical { get; set; }
}

/// <summary>
/// Risposta completa della query grafo.
/// </summary>
public sealed class RelationshipGraphQueryResponseDto
{
    public required string SubjectType { get; set; }
    public required string SubjectId { get; set; }
    public required IReadOnlyList<string> RequestedIncludes { get; set; }
    public required IReadOnlyList<RelationshipGraphNodeDto> Nodes { get; set; }
    public required IReadOnlyList<RelationshipGraphEdgeDto> Edges { get; set; }
}

/// <summary>
/// Capability grafo per backward compatibility.
/// </summary>
public sealed class RelationshipGraphCapabilitiesDto
{
    public required IReadOnlyList<string> SubjectTypes { get; set; }
    public required IReadOnlyList<string> Includes { get; set; }
}

/// <summary>
/// DTO capability per subject type specifico, organizzate per categoria.
/// </summary>
public sealed class RelationshipGraphCapabilitiesBySubjectTypeDto
{
    public required string SubjectType { get; set; }
    public required IReadOnlyList<FilterCategoryDto> FiltersByCategory { get; set; }
}

/// <summary>
/// Categoria di filtri disponibili.
/// </summary>
public sealed class FilterCategoryDto
{
    public required string Category { get; set; }
    public required int CategoryOrder { get; set; }
    public required IReadOnlyList<FilterDto> Filters { get; set; }
}

/// <summary>
/// Singolo filtro configurabile.
/// </summary>
public sealed class FilterDto
{
    public required string Id { get; set; }
    public required string Label { get; set; }
    public required string Description { get; set; }
    public required bool UsesDependencyDepth { get; set; }
    public required bool IsRecommended { get; set; }
}
