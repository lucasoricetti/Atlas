using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class DivisionGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "division_used_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "📦 Used Assets",
            UiDescription: "Shows the Assets connected to this Division through USES",
            UiOrder: 50,
            QueryFactory: GraphQueryBuilder.DirectOutgoing("Division", "USES", "Asset"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "division_owned_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "📦 Owned Assets",
            UiDescription: "Shows the Assets connected to this Division through OWNS",
            UiOrder: 51,
            QueryFactory: GraphQueryBuilder.DirectOutgoing("Division", "OWNS", "Asset"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "division_used_processes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "⚙️ Used Processes",
            UiDescription: "Shows the Processes connected to this Division through USES",
            UiOrder: 52,
            QueryFactory: GraphQueryBuilder.DirectOutgoing("Division", "USES", "Process"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "division_owned_processes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "⚙️ Owned Processes",
            UiDescription: "Shows the Processes connected to this Division through OWNS",
            UiOrder: 53,
            QueryFactory: GraphQueryBuilder.DirectOutgoing("Division", "OWNS", "Process"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "division_used_processes_acnmacroareas",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "🛡️ ACN Macro Areas of Used Processes",
            UiDescription: "Shows the ACN Macro Areas classifying the Processes connected to this Division through USES.",
            UiOrder: 54,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Division",
                new GraphStep("USES", StepDirection.Outgoing, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "division_owned_processes_acnmacroareas",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "division" },
            UiLabel: "🛡️ ACN Macro Areas of Owned Processes",
            UiDescription: "Shows the ACN Macro Areas classifying the Processes connected to this Division through OWNS.",
            UiOrder: 55,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Division",
                new GraphStep("OWNS", StepDirection.Outgoing, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);
    }
}