using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class ServiceGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "dependency_chain",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🗄️ Services the Subject depends on (depth)",
            UiDescription: "Shows the Services this Service depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_dependents_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🗄️ Services depending on the Subject (depth)",
            UiDescription: "Shows the Services that depend on this Service up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_composing_assets_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🧩 Assets composed by the Subject (depth)",
            UiDescription: "Shows the Assets composed by this Service up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "target"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_involved_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "⚙️ Processes directly involving the Subject",
            UiDescription: "Shows the Processes directly involving this Service.",
            UiOrder: 37,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Service",
                relationshipType: "INVOLVES",
                sourceLabel: "Process"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_direct_acnmacroareas",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving the Subject",
            UiDescription: "Shows the ACN Macro Areas classifying the Processes that directly involve this Service.",
            UiOrder: 38,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_direct_suppliers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🏭 Subject direct Suppliers",
            UiDescription: "Shows the Suppliers directly associated with this Service through Contract.",
            UiOrder: 51,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "service_direct_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛠️ Subject Settings",
            UiDescription: "Shows the Settings directly associated with this Service.",
            UiOrder: 52,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "hosting_providers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🖥️ Subject direct Hosts",
            UiDescription: "Shows CloudProvider and VirtualMachine nodes directly hosting this Service.",
            UiOrder: 53,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Service",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p1",
                    Steps: Array.Empty<GraphStep>()),
                new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "vm"))
                    }),
                new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "cp"))
                    })),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        // ========== ADVANCED SUPPLIERS ==========
        yield return new GraphFilterDefinition(
            Id: "service_dependency_suppliers_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🏭 Suppliers of Services the Subject depends on (depth)",
            UiDescription: "Shows the Suppliers of the Services this Service depends on up to the selected depth.",
            UiOrder: 13,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSuppliers);

        yield return new GraphFilterDefinition(
            Id: "service_dependents_suppliers_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🏭 Suppliers of Services depending on the Subject (depth)",
            UiDescription: "Shows the Suppliers of the Services that depend on this Service up to the selected depth.",
            UiOrder: 15,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSuppliers);

        // ========== ADVANCED HOSTS ==========
        yield return new GraphFilterDefinition(
            Id: "service_dependency_hosts_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🖥️ Hosts of Services the Subject depends on (depth)",
            UiDescription: "Shows the Hosts of the Services this Service depends on up to the selected depth.",
            UiOrder: 16,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Service",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1)
                    }),
                new GraphPath(
                    StartAlias: "depService",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "vm"))
                    }),
                new GraphPath(
                    StartAlias: "depService",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "cp"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedHosts);

        yield return new GraphFilterDefinition(
            Id: "service_dependents_hosts_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🖥️ Hosts of Services depending on the Subject (depth)",
            UiDescription: "Shows the Hosts of the Services that depend on this Service up to the selected depth.",
            UiOrder: 17,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Service",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1)
                    }),
                new GraphPath(
                    StartAlias: "depService",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "target"))
                    }),
                new GraphPath(
                    StartAlias: "depService",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "target"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedHosts);

        // ========== ADVANCED SETTINGS ==========
        yield return new GraphFilterDefinition(
            Id: "service_upstream_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛠️ Settings of Services the Subject depends on (depth)",
            UiDescription: "Shows the Settings of the Services this Service depends on up to the selected depth.",
            UiOrder: 18,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        yield return new GraphFilterDefinition(
            Id: "service_downstream_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛠️ Settings of Services depending on the Subject (depth)",
            UiDescription: "Shows the Settings of the Services that depend on this Service up to the selected depth.",
            UiOrder: 19,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        yield return new GraphFilterDefinition(
            Id: "service_container_assets_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛠️ Settings of Assets containing the Subject (depth)",
            UiDescription: "Shows the Settings of the Assets composed by this Service up to the selected depth.",
            UiOrder: 34,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        // ========== ADVANCED LOGIN TYPES ==========
        yield return new GraphFilterDefinition(
            Id: "service_container_assets_logintypes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🔐 Login Types of Assets containing the Subject (depth)",
            UiDescription: "Shows the Login Types of the Assets composed by this Service up to the selected depth.",
            UiOrder: 31,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_LOGIN_TYPE", StepDirection.Outgoing, new NodeSpec("LoginType", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedLoginTypes);

        // ========== ADVANCED DIVISIONS ==========
        yield return new GraphFilterDefinition(
            Id: "service_container_assets_divisions_owning_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🏢 Divisions owning Assets containing the Subject (depth)",
            UiDescription: "Shows the Divisions that OWN Assets composed by this Service up to the selected depth.",
            UiOrder: 32,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("OWNS", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);

        yield return new GraphFilterDefinition(
            Id: "service_container_assets_divisions_using_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🏢 Divisions using Assets containing the Subject (depth)",
            UiDescription: "Shows the Divisions that USE Assets composed by this Service up to the selected depth.",
            UiOrder: 33,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("USES", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);

        // ========== ADVANCED ASSETS ==========
        yield return new GraphFilterDefinition(
            Id: "service_upstream_assets_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🧩 Assets containing Services the Subject depends on (depth)",
            UiDescription: "Shows upstream Assets that contain the Services this Service depends on, following outgoing DEPENDS_ON.",
            UiOrder: 35,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAssets);

        yield return new GraphFilterDefinition(
            Id: "service_downstream_assets_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🧩 Assets containing Services depending on the Subject (depth)",
            UiDescription: "Shows downstream Assets that contain the Services depending on this Service, following incoming DEPENDS_ON.",
            UiOrder: 36,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAssets);

        // ========== ADVANCED PROCESSES ==========
        yield return new GraphFilterDefinition(
            Id: "service_upstream_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "⚙️ Processes involving Services the Subject depends on (depth)",
            UiDescription: "Shows the Processes involving Services this Service depends on up to the selected depth.",
            UiOrder: 38,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        yield return new GraphFilterDefinition(
            Id: "service_downstream_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "⚙️ Processes involving Services depending on the Subject (depth)",
            UiDescription: "Shows the Processes involving Services that depend on this Service up to the selected depth.",
            UiOrder: 39,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        yield return new GraphFilterDefinition(
            Id: "service_container_assets_processes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "⚙️ Processes of Assets containing the Subject (depth)",
            UiDescription: "Shows the Processes of the Assets composed by this Service up to the selected depth.",
            UiOrder: 40,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        // ========== ADVANCED ACN MACRO AREAS ==========
        yield return new GraphFilterDefinition(
            Id: "service_upstream_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving Services the Subject depends on (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying Processes that involve Services this Service depends on up to the selected depth.",
            UiOrder: 41,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);

        yield return new GraphFilterDefinition(
            Id: "service_downstream_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving Services depending on the Subject (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying Processes that involve Services depending on this Service up to the selected depth.",
            UiOrder: 42,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Service", "depService"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);

        yield return new GraphFilterDefinition(
            Id: "service_container_assets_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "service" },
            UiLabel: "🛡️ ACN Macro Areas of Processes of Assets containing the Subject (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying the Processes of the Assets composed by this Service up to the selected depth.",
            UiOrder: 43,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Service",
                new GraphStep("COMPOSED_BY", StepDirection.Incoming, new NodeSpec("Asset", "asset0")),
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "asset"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);
    }
}
