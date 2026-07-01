using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

/// <summary>
/// Central registry for all graph filters available to the Graph Explorer.
///
/// Filters are defined in domain-specific modules and
/// collected here to ensure:
/// - uniqueness
/// - discoverability
/// - subject-type compatibility checks
/// </summary>
public static class GraphFilterRegistry
{
    private static readonly IReadOnlyDictionary<string, GraphFilterDefinition> AllFilters =
        BuildAllFilters();

    /// <summary>
    /// Returns all registered filters keyed by filter id.
    /// Intended for internal usage and diagnostics.
    /// </summary>
    public static IReadOnlyDictionary<string, GraphFilterDefinition> GetAll() => AllFilters;

    /// <summary>
    /// Returns available filters for a given subject type,
    /// ordered by UI category and display order.
    /// </summary>
    public static IReadOnlyList<GraphFilterDefinition> GetForSubjectType(string subjectType) =>
        AllFilters.Values
            .Where(f => f.SupportedSubjectTypes.Contains(subjectType, StringComparer.OrdinalIgnoreCase))
            .OrderBy(f => f.Category)
            .ThenBy(f => f.UiOrder)
            .ToList();

    /// <summary>
    /// Tries to resolve a filter by its identifier (case-insensitive).
    /// </summary>
    public static bool TryGet(string filterId, out GraphFilterDefinition? filter) =>
        AllFilters.TryGetValue(filterId, out filter);

    /// <summary>
    /// Checks whether a filter is supported by a subject type.
    /// </summary>
    public static bool IsSupported(string filterId, string subjectType) =>
        TryGet(filterId, out var filter) &&
        filter!.SupportedSubjectTypes.Contains(subjectType, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns all filters belonging to a specific UI category.
    /// </summary>
    public static IReadOnlyList<GraphFilterDefinition> GetByCategory(FilterCategory category) =>
        AllFilters.Values
            .Where(f => f.Category == category)
            .OrderBy(f => f.UiOrder)
            .ToList();

    /// <summary>
    /// Builds the complete filter registry by aggregating
    /// domain-specific filter modules.
    ///
    /// Duplicate filter identifiers are rejected at startup.
    /// </summary>
    private static IReadOnlyDictionary<string, GraphFilterDefinition> BuildAllFilters()
    {
        var filters =
            AssetGraphFilters.Get()
            .Concat(ServiceGraphFilters.Get())
            .Concat(DivisionGraphFilters.Get())
            .Concat(SettingGraphFilters.Get())
            .Concat(LoginTypeGraphFilters.Get())
            .Concat(HostingGraphFilters.Get())
            .Concat(ContractGraphFilters.Get())
            .Concat(SupplierGraphFilters.Get())
            .Concat(ProcessGraphFilters.Get())
            .Concat(AcnMacroAreaGraphFilters.Get());

        var dictionary = new Dictionary<string, GraphFilterDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var filter in filters)
        {
            if (!dictionary.TryAdd(filter.Id, filter))
            {
                throw new InvalidOperationException(
                    $"Duplicate filter detected in registry: '{filter.Id}'.");
            }
        }

        return dictionary;
    }
}
