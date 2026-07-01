using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class SupplierGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "supplier_provided_contracts",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "supplier" },
            UiLabel: "📄 Provided Contracts",
            UiDescription: "Shows the Contracts provided by this Supplier",
            UiOrder: 72,
            QueryFactory: GraphQueryBuilder.DirectIncoming("Supplier", "PROVIDED_BY", "Contract"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "supplier_services",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "supplier" },
            UiLabel: "🗄️ Linked Services",
            UiDescription: "Shows the Services linked to this Supplier through Contracts",
            UiOrder: 73,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Supplier",
                new GraphStep("PROVIDED_BY", StepDirection.Incoming, new NodeSpec("Contract", "contract")),
                new GraphStep("HAS_CONTRACT", StepDirection.Incoming, new NodeSpec("Service", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        // ========== ADVANCED ASSETS ==========
        yield return new GraphFilterDefinition(
            Id: "supplier_assets",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "supplier" },
            UiLabel: "📦 Assets containing linked Services (depth)",
            UiDescription: "Shows the Assets containing the Services linked to this Supplier up to the selected depth.",
            UiOrder: 74,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Supplier",
                new GraphStep("PROVIDED_BY", StepDirection.Incoming, new NodeSpec("Contract", "contract")),
                new GraphStep("HAS_CONTRACT", StepDirection.Incoming, new NodeSpec("Service", "service")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAssets);

        // ========== ADVANCED HOSTS ==========
        yield return new GraphFilterDefinition(
            Id: "supplier_service_hosts",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "supplier" },
            UiLabel: "🖥️ Hosts of linked Services (depth)",
            UiDescription: "Shows the Hosts of the Services linked to this Supplier up to the selected depth.",
            UiOrder: 76,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Supplier",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                        new GraphStep("PROVIDED_BY", StepDirection.Incoming, new NodeSpec("Contract", "contract")),
                        new GraphStep("HAS_CONTRACT", StepDirection.Incoming, new NodeSpec("Service", "service")),
                        new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "target"))
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "target"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedHosts);
    }
}