using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class ProcessGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========

        yield return new GraphFilterDefinition(
            Id: "process_direct_acnmacroareas",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🛡️ Subject ACN Macro Areas",
            UiDescription: "Shows the ACN Macro Areas directly classifying this Process.",
            UiOrder: 1,
            QueryFactory: GraphQueryBuilder.DirectOutgoing(
                subjectLabel: "Process",
                relationshipType: "CLASSIFIED_AS",
                targetLabel: "AcnMacroArea"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "process_involves_assets_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "📦 Assets involved in the Subject",
            UiDescription: "Shows the Assets directly involved in this Process.",
            UiOrder: 2,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Process",
                new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Asset", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "process_involves_services_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🗄️ Services involved in the Subject",
            UiDescription: "Shows the Services directly involved in this Process.",
            UiOrder: 3,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Process",
                new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "process_direct_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🛠️ Subject Settings",
            UiDescription: "Shows the Settings directly associated with this Process.",
            UiOrder: 4,
            QueryFactory: GraphQueryBuilder.DirectOutgoing(
                subjectLabel: "Process",
                relationshipType: "HAS_SETTING",
                targetLabel: "Setting"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "process_divisions_owning_direct",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🏢 Divisions directly owning the Subject",
            UiDescription: "Shows the Divisions directly connected to this Process through OWNS.",
            UiOrder: 5,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Process",
                relationshipType: "OWNS",
                sourceLabel: "Division"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "process_divisions_using_direct",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🏢 Divisions directly using the Subject",
            UiDescription: "Shows the Divisions directly connected to this Process through USES.",
            UiOrder: 6,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Process",
                relationshipType: "USES",
                sourceLabel: "Division"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        // ========== ADVANCED ASSETS ==========
        yield return new GraphFilterDefinition(
            Id: "process_involved_assets_dependencies_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "📦 Assets that involved Assets depend on (depth)",
            UiDescription: "Shows the Assets that the Assets involved in this Process depend on up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Process",
                new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 1),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAssets);

        // ========== ADVANCED SERVICES ==========
        yield return new GraphFilterDefinition(
            Id: "process_involved_services_dependencies_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🗄️ Services that involved Services depend on (depth)",
            UiDescription: "Shows the Services that the Services involved in this Process depend on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Process",
                new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 1),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedServices);

        yield return new GraphFilterDefinition(
            Id: "process_involved_services_suppliers_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "process" },
            UiLabel: "🏭 Suppliers of Services involved in the Subject (depth)",
            UiDescription: "Shows the Suppliers of the Services involved in this Process up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Process",
                new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedServices);
    }
}
