using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class ContractGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "contract_used_by_services",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "contract" },
            UiLabel: "🗄️ Linked Services (depth)",
            UiDescription: "Shows the Services linked to this Contract up to the selected depth.",
            UiOrder: 70,
            QueryFactory: GraphQueryBuilder.LinearPath(
            "Contract",
            new GraphStep("HAS_CONTRACT", StepDirection.Incoming, new NodeSpec("Service", "directSvs")),
            new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "indirectSvs"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "contract_suppliers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "contract" },
            UiLabel: "🏭 Linked Suppliers",
            UiDescription: "Shows the Suppliers linked to this Contract",
            UiOrder: 71,
            QueryFactory: GraphQueryBuilder.DirectOutgoing("Contract", "PROVIDED_BY", "Supplier"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);
    }
}