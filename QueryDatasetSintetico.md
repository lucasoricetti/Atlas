// ============================================================
// ATLAS – Dataset di seed (dati sintetici, solo uso locale)
// ============================================================


// ----------------------------------------------------------
// DIVISIONS
// ----------------------------------------------------------
MERGE (n:Division {Name: "IT Infrastructure"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Division {Name: "Production"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Division {Name: "Human Resources"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Division {Name: "Finance"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Division {Name: "Security"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


// ----------------------------------------------------------
// SUPPLIERS
// ----------------------------------------------------------
MERGE (n:Supplier {Name: "Oracle"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Supplier {Name: "Microsoft"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Supplier {Name: "Accenture"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Supplier {Name: "Red Hat"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


MERGE (n:Supplier {Name: "VMware"})
ON CREATE SET
    n.Id = randomUUID()


WITH 1 AS dummy


// ----------------------------------------------------------
// VIRTUAL MACHINES
// ----------------------------------------------------------
MERGE (n:VirtualMachine {Name: "VMERP01"})
ON CREATE SET
    n.Id      = randomUUID(),
    n.Type    = "OnPrem",
    n.Ip      = "10.0.1.10",
    n.Cluster = "CLUSTER-PROD",
    n.Role    = "Application Server"


WITH 1 AS dummy


MERGE (n:VirtualMachine {Name: "VMDB01"})
ON CREATE SET
    n.Id      = randomUUID(),
    n.Type    = "OnPrem",
    n.Ip      = "10.0.1.11",
    n.Cluster = "CLUSTER-PROD",
    n.Role    = "Database Server"


WITH 1 AS dummy


MERGE (n:VirtualMachine {Name: "VMAUTH01"})
ON CREATE SET
    n.Id      = randomUUID(),
    n.Type    = "OnPrem",
    n.Ip      = "10.0.2.10",
    n.Cluster = "CLUSTER-AUTH",
    n.Role    = "Identity Server"


WITH 1 AS dummy


MERGE (n:VirtualMachine {Name: "VMHR01"})
ON CREATE SET
    n.Id      = randomUUID(),
    n.Type    = "OnPrem",
    n.Ip      = "10.0.3.10",
    n.Cluster = "CLUSTER-HR",
    n.Role    = "Application Server"


WITH 1 AS dummy


MERGE (n:VirtualMachine {Name: "VMMON01"})
ON CREATE SET
    n.Id      = randomUUID(),
    n.Type    = "OnPrem",
    n.Ip      = "10.0.4.10",
    n.Cluster = "CLUSTER-OPS",
    n.Role    = "Monitoring Server"


WITH 1 AS dummy


// ----------------------------------------------------------
// CLOUD PROVIDERS
// ----------------------------------------------------------
MERGE (n:CloudProvider {Name: "Azure"})
ON CREATE SET
    n.Id        = randomUUID(),
    n.Type      = "PaaS",
    n.PortalUrl = "https://portal.azure.com",
    n.Account   = "tenant-acme-prod"


WITH 1 AS dummy


MERGE (n:CloudProvider {Name: "AWS"})
ON CREATE SET
    n.Id        = randomUUID(),
    n.Type      = "PaaS",
    n.PortalUrl = "https://console.aws.amazon.com",
    n.Account   = "aws-acme-eu-west"


WITH 1 AS dummy


// ----------------------------------------------------------
// ASSETS
// ----------------------------------------------------------
MERGE (n:Asset {Name: "ERP Aziendale"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",
    n.Criticality = "Critical",
    n.Bia         = true,
    n.MtoH        = 4,
    n.RpoH        = 1,
    n.Description = "Sistema ERP centrale per la gestione operativa e produttiva"


WITH 1 AS dummy


MERGE (n:Asset {Name: "Portale HR"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",
    n.Criticality = "High",
    n.Bia         = true,
    n.MtoH        = 8,
    n.RpoH        = 2,
    n.Description = "Portale self-service per la gestione del personale"


WITH 1 AS dummy


MERGE (n:Asset {Name: "Piattaforma Finance"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",
    n.Criticality = "High",
    n.Bia         = true,
    n.MtoH        = 6,
    n.RpoH        = 2,
    n.Description = "Sistema di contabilità e reporting finanziario"


WITH 1 AS dummy


MERGE (n:Asset {Name: "Sistema di Monitoring"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "System",
    n.Criticality = "Medium",
    n.Bia         = false,
    n.MtoH        = 24,
    n.RpoH        = 4,
    n.Description = "Stack Prometheus + Grafana per il monitoraggio infrastrutturale"


WITH 1 AS dummy


MERGE (n:Asset {Name: "Identity Provider"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "System",
    n.Criticality = "Critical",
    n.Bia         = true,
    n.MtoH        = 2,
    n.RpoH        = 1,
    n.Description = "Microsoft Entra ID – autenticazione e autorizzazione aziendale"


WITH 1 AS dummy


MERGE (n:Asset {Name: "Document Management System"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",
    n.Criticality = "Low",
    n.Bia         = false,
    n.MtoH        = 48,
    n.RpoH        = 8,
    n.Description = "Sistema di gestione documentale interno"


WITH 1 AS dummy


// ----------------------------------------------------------
// SERVICES
// ----------------------------------------------------------
MERGE (n:Service {Name: "apiERP"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "API",
    n.Version      = "3.2.1",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "dbERP"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Database",
    n.Version      = "19c",
    n.ProtocolPort = "TCP - 1521",
    n.Port         = 1521,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "frontendERP"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Frontend",
    n.Version      = "2.8.0",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "authService"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Identity",
    n.Version      = "2.0.5",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "apiHR"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "API",
    n.Version      = "1.4.0",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "dbHR"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Database",
    n.Version      = "8.0",
    n.ProtocolPort = "TCP - 3306",
    n.Port         = 3306,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "financeAPI"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "API",
    n.Version      = "1.1.0",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "dbFinance"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Database",
    n.Version      = "15.4",
    n.ProtocolPort = "TCP - 5432",
    n.Port         = 5432,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "prometheus"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Monitoring",
    n.Version      = "2.48.0",
    n.ProtocolPort = "HTTP - 9090",
    n.Port         = 9090,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "grafana"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "PROD",
    n.Category     = "Monitoring",
    n.Version      = "10.2.3",
    n.ProtocolPort = "HTTPS - 3000",
    n.Port         = 3000,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "apiERP-test"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "TEST",
    n.Category     = "API",
    n.Version      = "3.3.0-rc1",
    n.ProtocolPort = "HTTPS - 443",
    n.Port         = 443,
    n.Status       = "Running"


WITH 1 AS dummy


MERGE (n:Service {Name: "dbERP-test"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Env          = "TEST",
    n.Category     = "Database",
    n.Version      = "19c",
    n.ProtocolPort = "TCP - 1521",
    n.Port         = 1521,
    n.Status       = "Running"


WITH 1 AS dummy


// ----------------------------------------------------------
// CONTRACTS
// ----------------------------------------------------------
MERGE (n:Contract {Name: "Oracle DB Support"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Support"],
    n.Sla           = 12,
    n.ContactEmail  = "support@oracle.com",
    n.StartDate     = "2023-01-01",
    n.EndDate       = "2025-12-31"


WITH 1 AS dummy


MERGE (n:Contract {Name: "Microsoft Entra SLA"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Subscription"],
    n.Sla           = 14,
    n.ContactEmail  = "tam@microsoft.com",
    n.StartDate     = "2024-01-01",
    n.EndDate       = "2026-12-31"


WITH 1 AS dummy


MERGE (n:Contract {Name: "Accenture API Managed"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Support"],
    n.Sla           = 8,
    n.ContactEmail  = "pm.acme@accenture.com",
    n.ContactPhone  = "+39 02 1234567",
    n.StartDate     = "2023-06-01",
    n.EndDate       = "2025-05-31"


WITH 1 AS dummy


MERGE (n:Contract {Name: "RHEL Enterprise Support"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Support"],
    n.Sla           = 6,
    n.ContactEmail  = "support@redhat.com",
    n.StartDate     = "2022-03-01",
    n.EndDate       = "2025-02-28"


WITH 1 AS dummy


MERGE (n:Contract {Name: "VMware vSphere License"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Warranty"],
    n.Sla           = 24,
    n.ContactEmail  = "licensing@vmware.com",
    n.StartDate     = "2023-09-01",
    n.EndDate       = "2026-08-31"


WITH 1 AS dummy


MERGE (n:Contract {Name: "Oracle DB HR Support"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Support"],
    n.Sla           = 10,
    n.ContactEmail  = "support@oracle.com",
    n.StartDate     = "2023-01-01",
    n.EndDate       = "2025-12-31"


WITH 1 AS dummy


// ----------------------------------------------------------
// LOGIN TYPES
// ----------------------------------------------------------
MERGE (n:LoginType {Name: "OIDC-EntraID"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Protocol = "OIDC",
    n.Mfa      = true


WITH 1 AS dummy


MERGE (n:LoginType {Name: "SAML-EntraID"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Protocol = "SAML",
    n.Mfa      = true


WITH 1 AS dummy


MERGE (n:LoginType {Name: "OAuth2-Local"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Protocol = "OAuth2",
    n.Mfa      = false


WITH 1 AS dummy


MERGE (n:LoginType {Name: "LDAP-Internal"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Protocol = "LDAP",
    n.Mfa      = false


WITH 1 AS dummy


// ----------------------------------------------------------
// SETTINGS
// ----------------------------------------------------------
MERGE (n:Setting {Name: "ERP-Wiki"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Links = ["https://wiki.acme.local/erp"],
    n.Description = "Documentazione tecnica ERP Aziendale"

WITH 1 AS dummy

MERGE (n:Setting {Name: "HR-AdminPanel"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Links = ["https://hr-admin.acme.local"],
    n.Description = "Pannello di amministrazione Portale HR"

WITH 1 AS dummy

MERGE (n:Setting {Name: "Grafana-Main"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Links = ["https://grafana.acme.local:3000"],
    n.Description = "Dashboard principale Grafana"

WITH 1 AS dummy

MERGE (n:Setting {Name: "Azure-Portal"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Links = ["https://portal.azure.com/#acme"],
    n.Description = "Portale Azure – tenant Acme"

WITH 1 AS dummy


// ----------------------------------------------------------
// PROCESSES
// ----------------------------------------------------------
MERGE (n:Process {Name: "ERP Order Management"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Description = "Processo di gestione ordini ERP"

WITH 1 AS dummy

MERGE (n:Process {Name: "HR Employee Self-Service"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Description = "Processo self-service per i dipendenti"

WITH 1 AS dummy

MERGE (n:Process {Name: "Finance Reporting"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Description = "Processo di reporting finanziario"

WITH 1 AS dummy

MERGE (n:Process {Name: "Infrastructure Monitoring"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Description = "Processo di monitoraggio infrastrutturale"

WITH 1 AS dummy

MERGE (n:Process {Name: "Identity Access Management"})
ON CREATE SET
    n.Id = randomUUID(),
    n.Description = "Processo di autenticazione e autorizzazione"

WITH 1 AS dummy


// ----------------------------------------------------------
// ACN MACRO AREAS
// ----------------------------------------------------------
MERGE (n:AcnMacroArea {Name: "ACN - Minimal Impact"})
ON CREATE SET
    n.Id = randomUUID(),
    n.PreAssignedAcnCategory = "Minimal_Impact",
    n.CustomAcnCategory = "Minimal_Impact"

WITH 1 AS dummy

MERGE (n:AcnMacroArea {Name: "ACN - Low Impact"})
ON CREATE SET
    n.Id = randomUUID(),
    n.PreAssignedAcnCategory = "Low_Impact",
    n.CustomAcnCategory = "Low_Impact"

WITH 1 AS dummy

MERGE (n:AcnMacroArea {Name: "ACN - Medium Impact"})
ON CREATE SET
    n.Id = randomUUID(),
    n.PreAssignedAcnCategory = "Medium_Impact",
    n.CustomAcnCategory = "Medium_Impact"

WITH 1 AS dummy

MERGE (n:AcnMacroArea {Name: "ACN - High Impact"})
ON CREATE SET
    n.Id = randomUUID(),
    n.PreAssignedAcnCategory = "High_Impact",
    n.CustomAcnCategory = "High_Impact"

WITH 1 AS dummy


// ============================================================
// ATLAS – Seed relazioni (dati sintetici, solo uso locale)
// Usa MATCH su Name (univoco per label) + MERGE sulla relazione
// per rendere lo script idempotente.
// ============================================================


// ----------------------------------------------------------
// Asset -[:COMPOSED_BY]-> Service
// ----------------------------------------------------------
MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Service {Name: "apiERP"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Service {Name: "dbERP"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Service {Name: "frontendERP"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Portale HR"}), (s:Service {Name: "apiHR"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Portale HR"}), (s:Service {Name: "dbHR"})
MERGE (a)-[:COMPOSED_BY {IsCritical: false}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Piattaforma Finance"}), (s:Service {Name: "financeAPI"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Piattaforma Finance"}), (s:Service {Name: "dbFinance"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Sistema di Monitoring"}), (s:Service {Name: "prometheus"})
MERGE (a)-[:COMPOSED_BY {IsCritical: false}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Sistema di Monitoring"}), (s:Service {Name: "grafana"})
MERGE (a)-[:COMPOSED_BY {IsCritical: false}]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Identity Provider"}), (s:Service {Name: "authService"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)


WITH 1 AS dummy


// ----------------------------------------------------------
// Service -[:DEPENDS_ON]-> Service
// ----------------------------------------------------------
MATCH (a:Service {Name: "frontendERP"}), (b:Service {Name: "apiERP"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "apiERP"}), (b:Service {Name: "dbERP"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "apiERP"}), (b:Service {Name: "authService"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "apiHR"}), (b:Service {Name: "dbHR"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "apiHR"}), (b:Service {Name: "authService"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "financeAPI"}), (b:Service {Name: "dbFinance"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Service {Name: "grafana"}), (b:Service {Name: "prometheus"})
MERGE (a)-[:DEPENDS_ON {IsCritical: false}]->(b)


WITH 1 AS dummy


// ----------------------------------------------------------
// Asset -[:DEPENDS_ON]-> Asset
// ----------------------------------------------------------
MATCH (a:Asset {Name: "ERP Aziendale"}), (b:Asset {Name: "Identity Provider"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Portale HR"}), (b:Asset {Name: "Identity Provider"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Piattaforma Finance"}), (b:Asset {Name: "Portale HR"})
MERGE (a)-[:DEPENDS_ON {IsCritical: false}]->(b)


WITH 1 AS dummy


// ----------------------------------------------------------
// VirtualMachine -[:HOSTS]-> Service
// ----------------------------------------------------------
MATCH (vm:VirtualMachine {Name: "VMERP01"}), (s:Service {Name: "apiERP"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMERP01"}), (s:Service {Name: "frontendERP"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMDB01"}), (s:Service {Name: "dbERP"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMDB01"}), (s:Service {Name: "dbHR"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMHR01"}), (s:Service {Name: "apiHR"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMAUTH01"}), (s:Service {Name: "authService"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMMON01"}), (s:Service {Name: "prometheus"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (vm:VirtualMachine {Name: "VMMON01"}), (s:Service {Name: "grafana"})
MERGE (vm)-[:HOSTS]->(s)


WITH 1 AS dummy


// ----------------------------------------------------------
// CloudProvider -[:HOSTS]-> Service
// ----------------------------------------------------------
MATCH (cp:CloudProvider {Name: "Azure"}), (s:Service {Name: "apiERP-test"})
MERGE (cp)-[:HOSTS]->(s)


WITH 1 AS dummy


MATCH (cp:CloudProvider {Name: "Azure"}), (s:Service {Name: "dbERP-test"})
MERGE (cp)-[:HOSTS]->(s)


WITH 1 AS dummy


// ----------------------------------------------------------
// Service -[:HAS_CONTRACT]-> Contract
// ----------------------------------------------------------
MATCH (s:Service {Name: "dbERP"}), (c:Contract {Name: "Oracle DB Support"})
MERGE (s)-[:HAS_CONTRACT]->(c)


WITH 1 AS dummy


MATCH (s:Service {Name: "authService"}), (c:Contract {Name: "Microsoft Entra SLA"})
MERGE (s)-[:HAS_CONTRACT]->(c)


WITH 1 AS dummy


MATCH (s:Service {Name: "apiERP"}), (c:Contract {Name: "Accenture API Managed"})
MERGE (s)-[:HAS_CONTRACT]->(c)


WITH 1 AS dummy


MATCH (s:Service {Name: "dbHR"}), (c:Contract {Name: "Oracle DB HR Support"})
MERGE (s)-[:HAS_CONTRACT]->(c)


WITH 1 AS dummy


// ----------------------------------------------------------
// Contract -[:PROVIDED_BY]-> Supplier
// ----------------------------------------------------------
MATCH (c:Contract {Name: "Oracle DB Support"}), (sup:Supplier {Name: "Oracle"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


MATCH (c:Contract {Name: "Microsoft Entra SLA"}), (sup:Supplier {Name: "Microsoft"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


MATCH (c:Contract {Name: "Accenture API Managed"}), (sup:Supplier {Name: "Accenture"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


MATCH (c:Contract {Name: "RHEL Enterprise Support"}), (sup:Supplier {Name: "Red Hat"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


MATCH (c:Contract {Name: "VMware vSphere License"}), (sup:Supplier {Name: "VMware"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


MATCH (c:Contract {Name: "Oracle DB HR Support"}), (sup:Supplier {Name: "Oracle"})
MERGE (c)-[:PROVIDED_BY]->(sup)


WITH 1 AS dummy


// ----------------------------------------------------------
// Division -[:OWNS]-> Asset
// ----------------------------------------------------------
MATCH (d:Division {Name: "IT Infrastructure"}), (a:Asset {Name: "ERP Aziendale"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "IT Infrastructure"}), (a:Asset {Name: "Sistema di Monitoring"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "IT Infrastructure"}), (a:Asset {Name: "Identity Provider"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Human Resources"}), (a:Asset {Name: "Portale HR"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Finance"}), (a:Asset {Name: "Piattaforma Finance"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Security"}), (a:Asset {Name: "Document Management System"})
MERGE (d)-[:OWNS]->(a)


WITH 1 AS dummy


// ----------------------------------------------------------
// Division -[:USES]-> Asset
// ----------------------------------------------------------
MATCH (d:Division {Name: "Production"}), (a:Asset {Name: "ERP Aziendale"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Human Resources"}), (a:Asset {Name: "ERP Aziendale"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Finance"}), (a:Asset {Name: "ERP Aziendale"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "IT Infrastructure"}), (a:Asset {Name: "Portale HR"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Finance"}), (a:Asset {Name: "Portale HR"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "Security"}), (a:Asset {Name: "Sistema di Monitoring"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


MATCH (d:Division {Name: "IT Infrastructure"}), (a:Asset {Name: "Piattaforma Finance"})
MERGE (d)-[:USES]->(a)


WITH 1 AS dummy


// ----------------------------------------------------------
// Asset -[:HAS_LOGIN_TYPE]-> LoginType
// ----------------------------------------------------------
MATCH (a:Asset {Name: "ERP Aziendale"}), (lt:LoginType {Name: "OIDC-EntraID"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Portale HR"}), (lt:LoginType {Name: "OIDC-EntraID"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Piattaforma Finance"}), (lt:LoginType {Name: "SAML-EntraID"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Document Management System"}), (lt:LoginType {Name: "LDAP-Internal"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Sistema di Monitoring"}), (lt:LoginType {Name: "OAuth2-Local"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)


WITH 1 AS dummy


// ----------------------------------------------------------
// Asset -[:HAS_SETTING]-> Setting
// ----------------------------------------------------------
MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Setting {Name: "ERP-Wiki"})
MERGE (a)-[:HAS_SETTING]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Portale HR"}), (s:Setting {Name: "HR-AdminPanel"})
MERGE (a)-[:HAS_SETTING]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Sistema di Monitoring"}), (s:Setting {Name: "Grafana-Main"})
MERGE (a)-[:HAS_SETTING]->(s)


WITH 1 AS dummy


MATCH (a:Asset {Name: "Identity Provider"}), (s:Setting {Name: "Azure-Portal"})
MERGE (a)-[:HAS_SETTING]->(s)


WITH 1 AS dummy


// ----------------------------------------------------------
// Process -[:HAS_SETTING]-> Setting
// ----------------------------------------------------------
MATCH (p:Process {Name: "ERP Order Management"}), (st:Setting {Name: "ERP-Wiki"})
MERGE (p)-[:HAS_SETTING]->(st)

WITH 1 AS dummy

MATCH (p:Process {Name: "HR Employee Self-Service"}), (st:Setting {Name: "HR-AdminPanel"})
MERGE (p)-[:HAS_SETTING]->(st)

WITH 1 AS dummy

MATCH (p:Process {Name: "Infrastructure Monitoring"}), (st:Setting {Name: "Grafana-Main"})
MERGE (p)-[:HAS_SETTING]->(st)

WITH 1 AS dummy

MATCH (p:Process {Name: "Identity Access Management"}), (st:Setting {Name: "Azure-Portal"})
MERGE (p)-[:HAS_SETTING]->(st)

WITH 1 AS dummy


// ----------------------------------------------------------
// Process -[:INVOLVES]-> Asset
// ----------------------------------------------------------
MATCH (p:Process {Name: "ERP Order Management"}), (a:Asset {Name: "ERP Aziendale"})
MERGE (p)-[:INVOLVES]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "HR Employee Self-Service"}), (a:Asset {Name: "Portale HR"})
MERGE (p)-[:INVOLVES]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Finance Reporting"}), (a:Asset {Name: "Piattaforma Finance"})
MERGE (p)-[:INVOLVES]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Infrastructure Monitoring"}), (a:Asset {Name: "Sistema di Monitoring"})
MERGE (p)-[:INVOLVES]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Identity Access Management"}), (a:Asset {Name: "Identity Provider"})
MERGE (p)-[:INVOLVES]->(a)

WITH 1 AS dummy


// ----------------------------------------------------------
// Process -[:INVOLVES]-> Service
// ----------------------------------------------------------
MATCH (p:Process {Name: "ERP Order Management"}), (s:Service {Name: "apiERP"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy

MATCH (p:Process {Name: "HR Employee Self-Service"}), (s:Service {Name: "apiHR"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy

MATCH (p:Process {Name: "Finance Reporting"}), (s:Service {Name: "financeAPI"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy

MATCH (p:Process {Name: "Infrastructure Monitoring"}), (s:Service {Name: "prometheus"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy

MATCH (p:Process {Name: "Infrastructure Monitoring"}), (s:Service {Name: "grafana"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy

MATCH (p:Process {Name: "Identity Access Management"}), (s:Service {Name: "authService"})
MERGE (p)-[:INVOLVES]->(s)

WITH 1 AS dummy


// ----------------------------------------------------------
// Process -[:CLASSIFIED_AS]-> AcnMacroArea
// ----------------------------------------------------------
MATCH (p:Process {Name: "ERP Order Management"}), (a:AcnMacroArea {Name: "ACN - High Impact"})
MERGE (p)-[:CLASSIFIED_AS]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "HR Employee Self-Service"}), (a:AcnMacroArea {Name: "ACN - Medium Impact"})
MERGE (p)-[:CLASSIFIED_AS]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Finance Reporting"}), (a:AcnMacroArea {Name: "ACN - High Impact"})
MERGE (p)-[:CLASSIFIED_AS]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Infrastructure Monitoring"}), (a:AcnMacroArea {Name: "ACN - Medium Impact"})
MERGE (p)-[:CLASSIFIED_AS]->(a)

WITH 1 AS dummy

MATCH (p:Process {Name: "Identity Access Management"}), (a:AcnMacroArea {Name: "ACN - High Impact"})
MERGE (p)-[:CLASSIFIED_AS]->(a)