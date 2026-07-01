using Atlas.Backend.Application.DTOs.Relationships;
using Atlas.Backend.Application.IRepositories.Relationships;
using Atlas.Backend.Application.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Application.Services;

/// <summary>
/// Application service responsible for:
/// - exposing graph capabilities
/// - validating graph queries
/// - orchestrating graph retrieval
///
/// This layer contains no persistence logic and no Cypher queries.
/// </summary>
public class RelationshipGraphService
{
    private readonly IRelationshipGraphRepository _repository;

    /// <summary>
    /// Creates the service using a graph repository abstraction.
    /// </summary>
    public RelationshipGraphService(IRelationshipGraphRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns legacy-style graph capabilities.
    ///
    /// This exposes only:
    /// - supported subject types
    /// - supported include identifiers
    /// </summary>
    public RelationshipGraphCapabilitiesDto GetCapabilities()
    {
        return new RelationshipGraphCapabilitiesDto
        {
            SubjectTypes = _repository.SupportedSubjectTypes
                .OrderBy(x => x)
                .ToList(),

            Includes = _repository.SupportedIncludes
                .OrderBy(x => x)
                .ToList()
        };
    }

    /// <summary>
    /// Returns a structured, UI-friendly description of
    /// available filters for a specific subject type.
    ///
    /// The subject type is normalized and validated before use.
    /// </summary>
    public RelationshipGraphCapabilitiesBySubjectTypeDto GetCapabilitiesBySubjectType(string subjectType)
    {
        var normalizedSubjectType = NormalizeSubjectType(subjectType);

        // Defensive check: fail fast if an unsupported subject type is requested
        EnsureSupportedSubjectType(normalizedSubjectType, subjectType);

        var filters = _repository.GetAvailableFilters(normalizedSubjectType);

        return new RelationshipGraphCapabilitiesBySubjectTypeDto
        {
            SubjectType = normalizedSubjectType,
            FiltersByCategory = BuildFiltersByCategory(filters)
        };
    }

    /// <summary>
    /// Executes a graph query starting from a subject node.
    ///
    /// Responsibilities of this method:
    /// - normalize and validate input
    /// - validate requested filters
    /// - invoke repository
    /// - convert domain objects to DTOs
    /// </summary>
    public async Task<RelationshipGraphQueryResponseDto> QueryAsync(
        RelationshipGraphQueryRequestDto request,
        CancellationToken ct = default)
    {
        var subjectType = request.SubjectType.Trim().ToLowerInvariant();
        var subjectId = request.SubjectId.Trim();

        EnsureSupportedSubjectType(subjectType, request.SubjectType);

        // Normalize and de-duplicate includes
        var requestedIncludes = request.Includes
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Ensure all requested filters are supported for this subject
        EnsureSupportedIncludes(subjectType, requestedIncludes);

        var subject = new RelationshipGraphSubject(subjectType, subjectId);

        // Repository executes all Cypher logic and graph expansion
        var graph = await _repository.QueryAsync(
            subject,
            requestedIncludes,
            request.DependencyDepth,
            ct);

        // Explicit mapping to DTOs to avoid leaking domain types
        return new RelationshipGraphQueryResponseDto
        {
            SubjectType = subjectType,
            SubjectId = subjectId,
            RequestedIncludes = requestedIncludes,
            Nodes = graph.Nodes.Select(x => new RelationshipGraphNodeDto
            {
                Id = x.Id,
                Type = x.Type,
                Label = x.Label
            }).ToList(),
            Edges = graph.Edges.Select(x => new RelationshipGraphEdgeDto
            {
                SourceId = x.SourceId,
                SourceType = x.SourceType,
                TargetId = x.TargetId,
                TargetType = x.TargetType,
                RelationType = x.RelationType,
                IsCritical = x.IsCritical
            }).ToList()
        };
    }

    /// <summary>
    /// Normalizes the subject type for internal usage.
    /// </summary>
    private static string NormalizeSubjectType(string subjectType)
    {
        return subjectType?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    /// <summary>
    /// Ensures that a subject type is supported by the repository.
    /// </summary>
    private void EnsureSupportedSubjectType(string normalizedSubjectType, string originalSubjectType)
    {
        if (string.IsNullOrWhiteSpace(normalizedSubjectType) ||
            !_repository.SupportedSubjectTypes.Contains(normalizedSubjectType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported SubjectType: '{originalSubjectType}'.");
        }
    }

    /// <summary>
    /// Validates that all requested filters are available for a given subject type.
    /// </summary>
    private void EnsureSupportedIncludes(string subjectType, IReadOnlyCollection<string> requestedIncludes)
    {
        var supported = _repository
            .GetAvailableFilters(subjectType)
            .Select(f => f.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unsupported = requestedIncludes
            .Where(x => !supported.Contains(x))
            .ToList();

        if (unsupported.Count > 0)
        {
            throw new ArgumentException(
                $"Unsupported includes for subject '{subjectType}': {string.Join(", ", unsupported)}");
        }
    }

    /// <summary>
    /// Groups filters by UI category and maps them to DTOs.
    /// </summary>
    private static List<FilterCategoryDto> BuildFiltersByCategory(
        IReadOnlyList<GraphFilterDefinition> filters)
    {
        return filters
            .GroupBy(f => f.Category)
            .OrderBy(g => g.Key)
            .Select(g => new FilterCategoryDto
            {
                Category = g.Key.ToString(),
                CategoryOrder = (int)g.Key,
                Filters = g
                    .OrderBy(f => f.UiOrder)
                    .Select(f => new FilterDto
                    {
                        Id = f.Id,
                        Label = f.UiLabel,
                        Description = f.UiDescription,
                        UsesDependencyDepth = f.UsesDependencyDepth,
                        IsRecommended = f.IsRecommended
                    })
                    .ToList()
            })
            .ToList();
    }
}