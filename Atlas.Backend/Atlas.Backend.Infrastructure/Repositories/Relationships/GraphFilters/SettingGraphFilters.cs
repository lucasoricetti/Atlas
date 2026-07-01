using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class SettingGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "setting_used_by_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "setting" },
            UiLabel: "🧩 Assets using this Setting",
            UiDescription: "Shows Assets directly linked through HAS_SETTING",
            UiOrder: 49,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Setting",
                relationshipType: "HAS_SETTING",
                sourceLabel: "Asset"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "setting_used_by_services",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "setting" },
            UiLabel: "🗄️ Services using this Setting",
            UiDescription: "Shows Services directly linked through HAS_SETTING",
            UiOrder: 50,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Setting",
                relationshipType: "HAS_SETTING",
                sourceLabel: "Service"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "setting_used_by_processes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "setting" },
            UiLabel: "⚙️ Processes using this Setting",
            UiDescription: "Shows Processes directly linked through HAS_SETTING",
            UiOrder: 51,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Setting",
                relationshipType: "HAS_SETTING",
                sourceLabel: "Process"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);
    }
}