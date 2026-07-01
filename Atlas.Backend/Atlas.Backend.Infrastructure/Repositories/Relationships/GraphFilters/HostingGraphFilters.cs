using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class HostingGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "cloudprovider_services",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cloudprovider" },
            UiLabel: "🗄️ Hosted Services (depth)",
            UiDescription: "Shows the Services hosted on this CloudProvider up to the selected depth.",
            UiOrder: 70,
            QueryFactory: GraphQueryBuilder.LinearPath(
            "CloudProvider",
            new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
            new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "indirectSvs"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "virtualmachine_services",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "virtualmachine" },
            UiLabel: "🗄️ Hosted Services (depth)",
            UiDescription: "Shows the Services hosted on this VirtualMachine up to the selected depth.",
            UiOrder: 71,
            QueryFactory: GraphQueryBuilder.LinearPath(
            "VirtualMachine",
            new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
            new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "indirectSvs"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "cloudprovider_service_suppliers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cloudprovider" },
            UiLabel: "🏭 Suppliers of hosted Services (depth)",
            UiDescription: "Shows the Suppliers linked to the Services hosted on this CloudProvider up to the selected depth.",
            UiOrder: 72,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "CloudProvider",
                new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
                UsesDependencyDepth: true,
                Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "virtualmachine_service_suppliers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "virtualmachine" },
            UiLabel: "🏭 Suppliers of hosted Services (depth)",
            UiDescription: "Shows the Suppliers linked to the Services hosted on this VirtualMachine up to the selected depth.",
            UiOrder: 73,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "VirtualMachine",
                new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
                UsesDependencyDepth: true,
                Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "cloudprovider_service_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cloudprovider" },
            UiLabel: "📦 Assets depending on hosted Services (depth)",
            UiDescription: "Shows the Assets linked to the Services hosted on this CloudProvider up to the selected depth.",
            UiOrder: 74,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "CloudProvider",
                new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "target"))),
                UsesDependencyDepth: true,
                Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "virtualmachine_service_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "virtualmachine" },
            UiLabel: "📦 Assets depending on hosted Services (depth)",
            UiDescription: "Shows the Assets linked to the Services hosted on this VirtualMachine up to the selected depth.",
            UiOrder: 74,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "VirtualMachine",
                new GraphStep("HOSTS", StepDirection.Outgoing, new NodeSpec("Service", "directSvs")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "service"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "target"))),
                UsesDependencyDepth: true,
                Category: FilterCategory.Main);
    }
}