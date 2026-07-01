using Atlas.Backend.Application.Definitions;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

internal static class AssetGraphFilters
{
    public static IEnumerable<GraphFilterDefinition> Get()
    {
        // ========== MAIN ==========
        yield return new GraphFilterDefinition(
            Id: "asset_dependencies_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "📦 Assets the Subject depends on (depth)",
            UiDescription: "Shows the Assets this Asset depends on up to the selected depth.",
            UiOrder: 1,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_dependents_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "📦 Assets depending on the Subject (depth)",
            UiDescription: "Shows the Assets that depend on this Asset up to the selected depth.",
            UiOrder: 2,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "target"), Recursive: true, MinDepth: 1)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_composed_services_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🗄️ Services composing the Subject (depth)",
            UiDescription: "Shows the Services composing this Asset up to the selected depth.",
            UiOrder: 3,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_direct_processes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "⚙️ Processes directly involving the Subject",
            UiDescription: "Shows the Processes directly involving this Asset.",
            UiOrder: 4,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Asset",
                relationshipType: "INVOLVES",
                sourceLabel: "Process"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_direct_acnmacroareas",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving the Subject",
            UiDescription: "Shows the ACN Macro Areas classifying the Processes that directly involve this Asset.",
            UiOrder: 5, 
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_composed_service_suppliers",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏭 Suppliers linked to Services composing the Subject (depth)",
            UiDescription: "Shows the Suppliers of the Services composing this Asset up to the selected depth.",
            UiOrder: 6,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_composed_service_hosts_main",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🖥️ Hosts of Services composing the Subject (depth)",
            UiDescription: "Shows CloudProvider and VirtualMachine nodes hosting the Services composing this Asset up to the selected depth.",
            UiOrder: 7,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Asset",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                        new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                        new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "vm"))
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "cp"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_direct_logintypes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🔐 Subject Login Types",
            UiDescription: "Shows the Login Types directly associated with this Asset.",
            UiOrder: 9,
            QueryFactory: GraphQueryBuilder.DirectOutgoing(
                subjectLabel: "Asset",
                relationshipType: "HAS_LOGIN_TYPE",
                targetLabel: "LoginType"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_direct_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛠️ Subject Settings",
            UiDescription: "Shows the Settings directly associated with this Asset.",
            UiOrder: 8,
            QueryFactory: GraphQueryBuilder.DirectOutgoing(
                subjectLabel: "Asset",
                relationshipType: "HAS_SETTING",
                targetLabel: "Setting"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_divisions_using_direct",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions directly using the Subject",
            UiDescription: "Shows the Divisions directly connected to this Asset through USES.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Asset",
                relationshipType: "USES",
                sourceLabel: "Division"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        yield return new GraphFilterDefinition(
            Id: "asset_divisions_owning_direct",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions directly owning the Subject",
            UiDescription: "Shows the Divisions directly connected to this Asset through OWNS.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.DirectIncoming(
                subjectLabel: "Asset",
                relationshipType: "OWNS",
                sourceLabel: "Division"),
            UsesDependencyDepth: false,
            Category: FilterCategory.Main);

        // ========== ADVANCED SERVICES ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_services_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🗄️ Services composing Assets the Subject depends on (depth)",
            UiDescription: "Shows the Services of the Assets this Asset depends on up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedServices);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_services_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🗄️ Services composing Assets depending on the Subject (depth)",
            UiDescription: "Shows the Services of the Assets that depend on this Asset up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "target"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedServices);

        // ========== ADVANCED PROCESSES ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "⚙️ Processes involving Assets the Subject depends on (depth)",
            UiDescription: "Shows the Processes involving Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "⚙️ Processes involving Assets depending on the Subject (depth)",
            UiDescription: "Shows the Processes involving Assets that depend on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        yield return new GraphFilterDefinition(
            Id: "asset_composed_service_processes_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "⚙️ Processes involving Services composing the Subject (depth)",
            UiDescription: "Shows the Processes involving Services that compose this Asset up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedProcesses);

        // ========== ADVANCED ACN MACRO AREAS ==========

        yield return new GraphFilterDefinition(
            Id: "asset_upstream_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving Assets the Subject depends on (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying Processes that involve Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving Assets depending on the Subject (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying Processes that involve Assets depending on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);

        yield return new GraphFilterDefinition(
            Id: "asset_composed_service_acnmacroareas_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛡️ ACN Macro Areas of Processes involving Services composing the Subject (depth)",
            UiDescription: "Shows the ACN Macro Areas classifying Processes that involve Services composing this Asset up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("INVOLVES", StepDirection.Incoming, new NodeSpec("Process", "proc")),
                new GraphStep("CLASSIFIED_AS", StepDirection.Outgoing, new NodeSpec("AcnMacroArea", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedAcnMacroAreas);

        // ========== ADVANCED SUPPLIERS ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_service_suppliers_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏭 Suppliers linked to Services composing Assets the Subject depends on (depth)",
            UiDescription: "Shows the Suppliers of Services belonging to Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSuppliers);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_service_suppliers_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏭 Suppliers linked to Services composing Assets depending on the Subject (depth)",
            UiDescription: "Shows the Suppliers of Services belonging to Assets that depend on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_CONTRACT", StepDirection.Outgoing, new NodeSpec("Contract", "contract")),
                new GraphStep("PROVIDED_BY", StepDirection.Outgoing, new NodeSpec("Supplier", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSuppliers);

        // ========== ADVANCED HOSTS ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_composed_service_hosts",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🖥️ Hosts of Services composing Assets the Subject depends on (depth)",
            UiDescription: "Shows CloudProvider and VirtualMachine nodes hosting the Services composing Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Asset",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                        new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                        new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                        new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "vm"))
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "cp"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedHosts);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_composed_service_hosts",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🖥️ Hosts of Services composing Assets depending on the Subject (depth)",
            UiDescription: "Shows CloudProvider and VirtualMachine nodes hosting the Services composing Assets that depend on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.MainPathWithBranches(
                subjectLabel: "Asset",
                mainPath: new GraphPath(
                    StartAlias: "subject",
                    PathAlias: "p",
                    Steps: new[]
                    {
                        new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                        new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                        new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true)
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p2",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("VirtualMachine", "vm"))
                    }),
                new GraphPath(
                    StartAlias: "svc",
                    PathAlias: "p3",
                    Steps: new[]
                    {
                        new GraphStep("HOSTS", StepDirection.Incoming, new NodeSpec("CloudProvider", "cp"))
                    })),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedHosts);

        // ========== ADVANCED SETTINGS ==========
        yield return new GraphFilterDefinition(
            Id: "asset_composed_service_settings_depth",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛠️ Settings of Services composing the Subject (depth)",
            UiDescription: "Shows the Settings of the Services composing this Asset up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("COMPOSED_BY", StepDirection.Outgoing, new NodeSpec("Service", "svc0")),
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Service", "svc"), Recursive: true, MinDepth: 0, UseDepthMinusOneAsUpperBound: true),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        yield return new GraphFilterDefinition(
            Id: "asset_upstream_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛠️ Settings of Assets the Subject depends on (depth)",
            UiDescription: "Shows the Settings of the Assets this Asset depends on up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_settings",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🛠️ Settings of Assets depending on the Subject (depth)",
            UiDescription: "Shows the Settings of the Assets that depend on this Asset up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_SETTING", StepDirection.Outgoing, new NodeSpec("Setting", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedSettings);

        // ========== ADVANCED LOGIN TYPES ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_logintypes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🔐 Login Types of Assets the Subject depends on (depth)",
            UiDescription: "Shows the Login Types of the Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_LOGIN_TYPE", StepDirection.Outgoing, new NodeSpec("LoginType", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedLoginTypes);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_logintypes",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🔐 Login Types of Assets depending on the Subject (depth)",
            UiDescription: "Shows the Login Types of the Assets that depend on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("HAS_LOGIN_TYPE", StepDirection.Outgoing, new NodeSpec("LoginType", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedLoginTypes);

        // ========== ADVANCED DIVISIONS ==========
        yield return new GraphFilterDefinition(
            Id: "asset_upstream_divisions_using",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions using Assets the Subject depends on (depth)",
            UiDescription: "Shows the Divisions that USE Assets this Asset depends on up to the selected depth.",
            UiOrder: 10,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("USES", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_divisions_using",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions using Assets depending on the Subject (depth)",
            UiDescription: "Shows the Divisions that USE Assets depending on this Asset up to the selected depth.",
            UiOrder: 11,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("USES", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);

        yield return new GraphFilterDefinition(
            Id: "asset_upstream_divisions_owning",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions owning Assets the Subject depends on (depth)",
            UiDescription: "Shows the Divisions that OWN Assets this Asset depends on up to the selected depth.",
            UiOrder: 12,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Outgoing, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("OWNS", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);

        yield return new GraphFilterDefinition(
            Id: "asset_downstream_divisions_owning",
            SupportedSubjectTypes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "asset" },
            UiLabel: "🏢 Divisions owning Assets depending on the Subject (depth)",
            UiDescription: "Shows the Divisions that OWN Assets depending on this Asset up to the selected depth.",
            UiOrder: 13,
            QueryFactory: GraphQueryBuilder.LinearPath(
                "Asset",
                new GraphStep("DEPENDS_ON", StepDirection.Incoming, new NodeSpec("Asset", "depAsset"), Recursive: true, MinDepth: 1),
                new GraphStep("OWNS", StepDirection.Incoming, new NodeSpec("Division", "target"))),
            UsesDependencyDepth: true,
            Category: FilterCategory.AdvancedDivisions);
    }
}