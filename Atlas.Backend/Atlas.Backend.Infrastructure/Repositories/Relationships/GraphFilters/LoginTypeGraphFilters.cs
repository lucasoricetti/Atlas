using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class LoginTypeGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "logintype_used_by_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "logintype" },
            UiLabel: "🧩 Assets using this LoginType",
            UiDescription: "Shows the Assets directly linked through HAS_LOGIN_TYPE",
            UiOrder: 49,
            QueryFactory: GraphQueryBuilder.DirectIncoming("LoginType", "HAS_LOGIN_TYPE", "Asset"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "logintype_divisions_owning_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "logintype" },
            UiLabel: "🏢 Divisions owning Assets using this LoginType",
            UiDescription: "Shows the Divisions that own Assets linked to this LoginType.",
            UiOrder: 51,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "LoginType",
                new GraphStep("HAS_LOGIN_TYPE", StepDirection.Incoming, new NodeSpec("Asset", "asset")),
                new GraphStep("OWNS", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "logintype_divisions_using_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "logintype" },
            UiLabel: "🏢 Divisions using Assets with this LoginType",
            UiDescription: "Shows the Divisions that use Assets linked to this LoginType.",
            UiOrder: 52,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "LoginType",
                new GraphStep("HAS_LOGIN_TYPE", StepDirection.Incoming, new NodeSpec("Asset", "asset")),
                new GraphStep("USES", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);
    }
}