namespace Atlas.Backend.Application.Definitions;

/// <summary>
/// Definition of a graph filter used by the Graph Explorer.
///
/// Each filter encapsulates:
/// - UI metadata
/// - supported subject types
/// - a Cypher query factory
/// </summary>
public sealed record GraphFilterDefinition(
    /// <summary>
    /// Unique identifier of the filter (used by the API and UI).
    /// Example: "outgoing_dependencies".
    /// </summary>
    string Id,

    /// <summary>
    /// Set of subject types that support this filter.
    /// </summary>
    IReadOnlySet<string> SupportedSubjectTypes,

    /// <summary>
    /// Human-readable UI label.
    /// </summary>
    string UiLabel,

    /// <summary>
    /// Short description shown as tooltip in the UI.
    /// </summary>
    string UiDescription,

    /// <summary>
    /// Display order within its category.
    /// </summary>
    int UiOrder,

    /// <summary>
    /// Factory that produces the Cypher query.
    ///
    /// Parameters:
    /// - string: subject label override (if any)
    /// - int: dependency depth
    /// </summary>
    Func<string, int, string> QueryFactory,

    /// <summary>
    /// Indicates whether this filter uses the dependency depth parameter.
    /// </summary>
    bool UsesDependencyDepth = false,

    /// <summary>
    /// Indicates whether the filter is recommended for standard usage.
    /// </summary>
    bool IsRecommended = true,

    /// <summary>
    /// UI category used for grouping filters visually.
    /// </summary>
    FilterCategory Category = FilterCategory.Main);

/// <summary>
/// UI filter categories used to group filters in the frontend.
/// </summary>
public enum FilterCategory
{
    Main = 0,
    AdvancedAssets = 10,
    AdvancedServices = 20,
    AdvancedProcesses = 25,
    AdvancedContracts = 30,
    AdvancedSuppliers = 40,
    AdvancedHosts = 50,
    AdvancedDivisions = 60,
    AdvancedAcnMacroAreas = 65,
    AdvancedSettings = 70,
    AdvancedLoginTypes = 80
}