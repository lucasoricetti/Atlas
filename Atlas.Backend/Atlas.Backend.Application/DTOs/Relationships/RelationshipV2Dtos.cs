using FluentValidation;
using System.Collections.Generic;

namespace Atlas.Backend.Application.DTOs.Relationships;

/// <summary>
/// Relazione attiva restituita dagli endpoint v2.
/// </summary>
public sealed class RelationshipV2ListItemDto
{
    public required string RelationId { get; set; }
    public required string TargetId { get; set; }
    public required string TargetLabel { get; set; }
    public bool? IsCritical { get; set; }
}

/// <summary>
/// Candidato collegabile ad una relazione v2.
/// </summary>
public sealed class RelationshipV2CandidateItemDto
{
    public required string TargetId { get; set; }
    public required string TargetLabel { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
}

/// <summary>
/// Risposta paginata dei candidati.
/// </summary>
public sealed class RelationshipV2CandidatesResponseDto
{
    public required IReadOnlyList<RelationshipV2CandidateItemDto> Items { get; set; }
    public string? NextCursor { get; set; }
    public long? TotalApprox { get; set; }
}

/// <summary>
/// Payload richiesta creazione relazione.
/// </summary>
public sealed class RelationshipV2AddRequestDto
{
    public string TargetId { get; set; } = null!;
    public bool? IsCritical { get; set; }
}

public sealed class RelationshipV2AddRequestDtoValidator : AbstractValidator<RelationshipV2AddRequestDto>
{
    public RelationshipV2AddRequestDtoValidator()
    {
        RuleFor(x => x.TargetId).NotEmpty().WithMessage("The TargetId field is required.");
    }
}

/// <summary>
/// Payload richiesta aggiornamento relazione.
/// </summary>
public sealed class RelationshipV2UpdateRequestDto
{
    public bool? IsCritical { get; set; }
}

public sealed class RelationshipV2UpdateRequestDtoValidator : AbstractValidator<RelationshipV2UpdateRequestDto>
{
    public RelationshipV2UpdateRequestDtoValidator()
    {
        RuleFor(x => x.IsCritical).NotNull().WithMessage("The IsCritical field is required.");
    }
}

/// <summary>
/// Richiesta batch per operazioni multiple.
/// </summary>
public sealed class RelationshipV2BatchRequestDto
{
    public List<RelationshipV2BatchOperationDto> Operations { get; set; } = new();
}

/// <summary>
/// Singola operazione batch.
/// </summary>
public sealed class RelationshipV2BatchOperationDto
{
    public string Op { get; set; } = null!;
    public string? TargetId { get; set; }
    public string? RelationId { get; set; }
    public bool? IsCritical { get; set; }
}

public sealed class RelationshipV2BatchRequestDtoValidator : AbstractValidator<RelationshipV2BatchRequestDto>
{
    public RelationshipV2BatchRequestDtoValidator()
    {
        RuleFor(x => x.Operations)
            .NotEmpty().WithMessage("The Operations field is required.");
    }
}

/// <summary>
/// Riepilogo conteggi batch.
/// </summary>
public sealed class RelationshipV2BatchSummaryDto
{
    public int Added { get; set; }
    public int Removed { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
}

/// <summary>
/// Esito batch con riepilogo e stato relazioni aggiornato.
/// </summary>
public sealed class RelationshipV2BatchResponseDto
{
    public required RelationshipV2BatchSummaryDto Summary { get; set; }
    public required IReadOnlyList<RelationshipV2ListItemDto> CurrentRelations { get; set; }
}
