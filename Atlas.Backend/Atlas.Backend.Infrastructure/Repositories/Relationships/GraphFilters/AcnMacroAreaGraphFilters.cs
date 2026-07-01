using Atlas.Backend.Application.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;
    internal static class AcnMacroAreaGraphFilters
    {
        public static IEnumerable<GraphFilterDefinition> Get()
        {
            // ========== MAIN ==========
            yield return new GraphFilterDefinition(
                Id: "acnmacroarea_direct_processes",
                SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "acnmacroarea" },
                UiLabel: "⚙️ Processes classified in this Macro Area",
                UiDescription: "Shows the Processes directly classified as this ACN Macro Area.",
                UiOrder: 1,
                QueryFactory: GraphQueryBuilder.DirectIncoming(
                    subjectLabel: "AcnMacroArea",
                    relationshipType: "CLASSIFIED_AS",
                    sourceLabel: "Process"),
                UsesDependencyDepth: false,
                Category: FilterCategory.Main);

            yield return new GraphFilterDefinition(
                Id: "acnmacroarea_involved_assets",
                SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "acnmacroarea" },
                UiLabel: "📦 Assets involved in classified Processes",
                UiDescription: "Shows the Assets involved in the Processes classified as this ACN Macro Area.",
                UiOrder: 2,
                QueryFactory: GraphQueryBuilder.LinearPath(
                    "AcnMacroArea",
                    new GraphStep("CLASSIFIED_AS", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                    new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Asset", "target"))),
                UsesDependencyDepth: false,
                Category: FilterCategory.Main);

            yield return new GraphFilterDefinition(
                Id: "acnmacroarea_involved_services",
                SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "acnmacroarea" },
                UiLabel: "🗄️ Services involved in classified Processes",
                UiDescription: "Shows the Services involved in the Processes classified as this ACN Macro Area.",
                UiOrder: 3,
                QueryFactory: GraphQueryBuilder.LinearPath(
                    "AcnMacroArea",
                    new GraphStep("CLASSIFIED_AS", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                    new GraphStep("INVOLVES", StepDirection.Outgoing, new NodeSpec("Service", "target"))),
                UsesDependencyDepth: false,
                Category: FilterCategory.Main);

            yield return new GraphFilterDefinition(
                Id: "acnmacroarea_divisions_using",
                SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "acnmacroarea" },
                UiLabel: "🏢 Divisions using classified Processes",
                UiDescription: "Shows the Divisions that USE Processes classified as this ACN Macro Area.",
                UiOrder: 4,
                QueryFactory: GraphQueryBuilder.LinearPath(
                    "AcnMacroArea",
                    new GraphStep("CLASSIFIED_AS", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                    new GraphStep("USES", StepDirection.Incoming, new NodeSpec("Division", "target"))),
                UsesDependencyDepth: false,
                Category: FilterCategory.Main);

            yield return new GraphFilterDefinition(
                Id: "acnmacroarea_divisions_owning",
                SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "acnmacroarea" },
                UiLabel: "🏢 Divisions owning classified Processes",
                UiDescription: "Shows the Divisions that OWN Processes classified as this ACN Macro Area.",
                UiOrder: 5,
                QueryFactory: GraphQueryBuilder.LinearPath(
                    "AcnMacroArea",
                    new GraphStep("CLASSIFIED_AS", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                    new GraphStep("OWNS", StepDirection.Incoming, new NodeSpec("Division", "target"))),
                UsesDependencyDepth: false,
                Category: FilterCategory.Main);
        }
    }
