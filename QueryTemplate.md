# Linee guida per la creazione dei nodi in Neo4j (Cypher)

Questo documento descrive come creare **nodi** e **relazioni** nel database Neo4j tramite Cypher, in modo coerente con il modello di dominio di Atlas.  

> 💡 **Regola generale**:  
> Ogni nodo richiede un campo `Id` in formato GUID, generato con `randomUUID()` direttamente in Cypher.  
> Il `MERGE` avviene sempre su `Name` (che ha un constraint di unicità): se il nodo esiste già, `ON CREATE SET` viene saltato e il nodo non viene duplicato.

> ⚠️ **Importante**:  
> Questo file è **sincronizzato con il modello di dominio** dell’applicazione.  
> In caso di modifiche alla struttura dei nodi o delle relazioni (campi, tipi, vincoli), **questo documento deve essere aggiornato di conseguenza**.

---

## Indice

- [Linee guida generali](#linee-guida-generali)
- [Creazione dei nodi](#creazione-dei-nodi)
  - [Asset](#asset)
  - [Service](#service)
  - [VirtualMachine](#virtualmachine)
  - [Contract](#contract)
  - [Supplier](#supplier)
  - [CloudProvider](#cloudprovider)
  - [LoginType](#logintype)
  - [Division](#division)
  - [Setting](#setting)
  - [Template multi‑nodo](#template-multi-nodo)
- [Creazione delle relazioni](#creazione-delle-relazioni)
  - [Struttura base delle query](#struttura-base-delle-query)
  - [Elenco delle relazioni supportate](#elenco-delle-relazioni-supportate)
  - [Script multi‑relazione](#script-multi-relazione)

---

## Linee guida generali

- Tutti i nodi devono avere:
  - `Id` (GUID generato con `randomUUID()`).
  - `Name` (obbligatorio, usato come criterio di unicità del nodo).
- Le proprietà opzionali possono essere omesse dallo script `ON CREATE SET` se non necessarie.
- Le relazioni sono sempre create con `MATCH` + `MERGE`, per garantire che:
  - i nodi esistano già,
  - le relazioni siano idempotenti (non create più volte).

---

## Creazione dei nodi

### Asset

Campi obbligatori: `Id`, `Name`, `Type`, `Criticality`, `Bia`  
Campi opzionali: `MtoH`, `RpoH`, `Description`

```cypher
MERGE (n:Asset {Name: "NomeAsset"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",      // "System" | "Application"
    n.Criticality = "High",             // "Critical" | "High" | "Medium" | "Low"
    n.Bia         = true,               // true | false
    n.MtoH        = 4,                  // OPZIONALE - int
    n.RpoH        = 2,                  // OPZIONALE - int
    n.Description = "Descrizione..."    // OPZIONALE — rimuovi se non necessario
```

---

### Service

Campi obbligatori: `Id`, `Name`, `Env`  
Campi opzionali: `Category`, `Version`, `Protocol`, `Port`, `Status`, `Description`

```cypher
MERGE (n:Service {Name: "NomeServizio"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.Env           = "PROD",           // "DEV" | "TEST" | "PROD"
    n.Category      = "Backend",        // OPZIONALE
    n.Version       = "1.0.0",          // OPZIONALE
    n.ProtocolPort  = "TCP - 443",      // OPZIONALE
    n.Description   = "Descrizione...", // OPZIONALE
    n.Status        = "Running"         // OPZIONALE — "Running" | "Stopped" | "Degraded" | "Maintenance" | "Provisioning" | "Deprecated" | "Retired"
```

---

---

### Process

Campi obbligatori: `Id`, `Name` 
Campi opzionali: `Description`

```cypher
MERGE (n:Process {Name: "NomeProcesso"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.Description   = "Descrizione..."   // OPZIONALE
```

---

### VirtualMachine

Campi obbligatori: `Id`, `Name`, `Type`  
Campi opzionali: `Ip`, `Cluster`, `Role`

```cypher
MERGE (n:VirtualMachine {Name: "NomeVM"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Type     = "OnPrem",              // "OnPrem" | "IaaS"
    n.Ip       = "192.168.1.100",       // OPZIONALE — IPv4 valido
    n.Cluster  = "cluster-01",          // OPZIONALE
    n.Role     = "App Server"           // OPZIONALE
```

---

### Contract

Campi obbligatori: `Id`, `Name`, `ContractTypes`  
Campi opzionali: `Sla`, `ContactEmail`, `ContactPhone`, `StartDate`, `EndDate`, `Notes`

```cypher
MERGE (n:Contract {Name: "NomeContratto"})
ON CREATE SET
    n.Id            = randomUUID(),
    n.ContractTypes = ["Support", "Warranty"],            // Array — valori: "Support" | "Warranty" | "Subscription" | "Other"
    n.Sla           = 99,                                 // OPZIONALE — int
    n.ContactEmail  = "referente@esempio.com",            // OPZIONALE — email valida
    n.ContactPhone  = "+39 333 1234567",                  // OPZIONALE — numero valido
    n.StartDate     = "2026-01-01",                       // OPZIONALE — formato ISO
    n.EndDate       = "2027-01-01",                       // OPZIONALE — formato ISO
    n.Notes         = "Note aggiuntive..."                // OPZIONALE
```

---

### Supplier

Campi obbligatori: `Id`, `Name`

```cypher
MERGE (n:Supplier {Name: "NomeFornitore"})
ON CREATE SET
    n.Id = randomUUID()
```

---

### CloudProvider

Campi obbligatori: `Id`, `Name`, `Type`  
Campi opzionali: `PortalUrl`, `Account`

```cypher
MERGE (n:CloudProvider {Name: "NomeProvider"})
ON CREATE SET
    n.Id        = randomUUID(),
    n.Type      = "SaaS",                                 // "PaaS" | "SaaS"
    n.PortalUrl = "https://portal.esempio.com",           // OPZIONALE — URL valido
    n.Account   = "account-id-123"                        // OPZIONALE
```

---

### LoginType

Campi obbligatori: `Id`, `Name`, `Protocol`, `Mfa`

```cypher
MERGE (n:LoginType {Name: "NomeLoginType"})
ON CREATE SET
    n.Id       = randomUUID(),
    n.Protocol = "OAuth2",                                // es. "SAML" | "OAuth2" | "LDAP" | "Local" ecc.
    n.Mfa      = true                                     // true | false
```

---

### Division

Campi obbligatori: `Id`, `Name`

```cypher
MERGE (n:Division {Name: "NomeDivisione"})
ON CREATE SET
    n.Id = randomUUID()
```

---

---

### Acn Macro Area

Campi obbligatori: `Id`, `Name`, `PreAssignedAcnCategory`
Campi opzionali: `CustomAcnCategory`

```cypher
MERGE (n:AcnMacroArea {Name: "NomeAcnMacroArea"})
ON CREATE SET
    n.Id = randomUUID(),
    n.PreAssignedAcnCategory      = "Minimal_Impact",               // "High_Impact" | "Medium_Impact" | "Low_Impact" | "Minimal_Impact"
    n.CustomAcnCategory           = "High_Impact",                  // OPZIONALE - "High_Impact" | "Medium_Impact" | "Low_Impact" | "Minimal_Impact"
```

---

### Setting

Campi obbligatori: `Id`, `Name`, `Links`  
Campi opzionali: `Description`

```cypher
MERGE (n:Setting {Name: "NomeSetting"})
ON CREATE SET
    n.Id           = randomUUID(),
    n.Links = ['https://link-valido.com', 'https://altro-link.com'],           // array di url validi (almeno un url)
    n.Description  = "Descrizione..."                                          // OPZIONALE
```

---

## Template multi‑nodo

Per inserire più nodi della stessa (o diverse) Label in una sola esecuzione, separa ogni blocco con `WITH 1 AS dummy`:

```cypher
MERGE (n:Asset {Name: "Asset Uno"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "Application",
    n.Criticality = "High",
    n.Bia         = true,
    n.MtoH        = 4,
    n.RpoH        = 2

WITH 1 AS dummy

MERGE (n:Asset {Name: "Asset Due"})
ON CREATE SET
    n.Id          = randomUUID(),
    n.Type        = "System",
    n.Criticality = "Medium",
    n.Bia         = false,
    n.MtoH        = 8,
    n.RpoH        = 8

WITH 1 AS dummy

MERGE (n:Supplier {Name: "Fornitore Alfa"})
ON CREATE SET
    n.Id = randomUUID()
```

> Puoi mixare Label diverse nello stesso script, basta separare ogni blocco con `WITH 1 AS dummy`.  
> Se un nodo esiste già (stesso `Name`), viene saltato silenziosamente senza bloccare le istruzioni successive.

---

## Creazione delle relazioni

> ⚠️ **Prerequisito**:  
> I nodi sorgente e destinazione devono **esistere già** nel database prima di eseguire le query di relazione.  
> Crea prima i nodi con `MERGE`, poi le relazioni.

Le relazioni si creano con il pattern `MATCH` + `MERGE`:
- `MATCH` individua i due nodi esistenti (senza crearli),
- `MERGE` crea la relazione solo se non esiste già, rendendo lo script **idempotente**.

### Struttura base delle query

```cypher
MATCH (a:LabelA {Name: "NodoA"}), (b:LabelB {Name: "NodoB"})
MERGE (a)-[:TIPO_RELAZIONE]->(b)
```

Per aggiungere proprietà alla relazione:

```cypher
MATCH (a:LabelA {Name: "NodoA"}), (b:LabelB {Name: "NodoB"})
MERGE (a)-[:TIPO_RELAZIONE {IsCritical: true}]->(b)
```

---

### Elenco delle relazioni supportate

#### `Asset -[:COMPOSED_BY]-> Service`

Un Asset è composto da uno o più Service.

| Proprietà    | Tipo | Obbligatoria | Valori              |
|-------------|------|-------------|---------------------|
| `IsCritical`| bool | Sì          | `true` \| `false`   |

```cypher
MATCH (a:Asset {Name: "NomeAsset"}), (s:Service {Name: "NomeServizio"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)
```

---

#### `Asset -[:DEPENDS_ON]-> Asset`

Un Asset dipende da un altro Asset per il suo funzionamento.

| Proprietà    | Tipo | Obbligatoria | Valori              |
|-------------|------|-------------|---------------------|
| `IsCritical`| bool | Sì          | `true` \| `false`   |

```cypher
MATCH (a:Asset {Name: "AssetDipendente"}), (b:Asset {Name: "AssetDipendenza"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)
```

---

#### `Service -[:DEPENDS_ON]-> Service`

Un Service dipende da un altro Service (es. frontend → API, API → database).

| Proprietà    | Tipo | Obbligatoria | Valori              |
|-------------|------|-------------|---------------------|
| `IsCritical`| bool | Sì          | `true` \| `false`   |

```cypher
MATCH (a:Service {Name: "ServizioDipendente"}), (b:Service {Name: "ServizioDipendenza"})
MERGE (a)-[:DEPENDS_ON {IsCritical: true}]->(b)
```

---

#### `VirtualMachine -[:HOSTS]-> Service`

Una VirtualMachine ospita uno o più Service (nessuna proprietà aggiuntiva).

```cypher
MATCH (vm:VirtualMachine {Name: "NomeVM"}), (s:Service {Name: "NomeServizio"})
MERGE (vm)-[:HOSTS]->(s)
```

---

#### `CloudProvider -[:HOSTS]-> Service`

Un CloudProvider ospita uno o più Service (es. ambienti cloud o test).

```cypher
MATCH (cp:CloudProvider {Name: "NomeProvider"}), (s:Service {Name: "NomeServizio"})
MERGE (cp)-[:HOSTS]->(s)
```

---

#### `Service -[:HAS_CONTRACT]-> Contract`

Un Service è coperto da un Contract.

```cypher
MATCH (s:Service {Name: "NomeServizio"}), (c:Contract {Name: "NomeContratto"})
MERGE (s)-[:HAS_CONTRACT]->(c)
```

---

#### `Contract -[:PROVIDED_BY]-> Supplier`

Un Contract è fornito da un Supplier.

```cypher
MATCH (c:Contract {Name: "NomeContratto"}), (sup:Supplier {Name: "NomeFornitore"})
MERGE (c)-[:PROVIDED_BY]->(sup)
```

---

#### `Division -[:OWNS]-> Asset`

Una Division è proprietaria (responsabile) di un Asset.

```cypher
MATCH (d:Division {Name: "NomeDivisione"}), (a:Asset {Name: "NomeAsset"})
MERGE (d)-[:OWNS]->(a)
```

---

#### `Division -[:USES]-> Asset`

Una Division utilizza un Asset senza esserne proprietaria.

```cypher
MATCH (d:Division {Name: "NomeDivisione"}), (a:Asset {Name: "NomeAsset"})
MERGE (d)-[:USES]->(a)
```

---

#### `Asset -[:HAS_LOGIN_TYPE]-> LoginType`

Un Asset utilizza un determinato metodo di autenticazione.

```cypher
MATCH (a:Asset {Name: "NomeAsset"}), (lt:LoginType {Name: "NomeLoginType"})
MERGE (a)-[:HAS_LOGIN_TYPE]->(lt)
```

---

#### `Asset -[:HAS_SETTING]-> Setting`

Un Asset è associato a un Setting (es. link a wiki, pannello di admin, portale esterno).

```cypher
MATCH (a:Asset {Name: "NomeAsset"}), (s:Setting {Name: "NomeSetting"})
MERGE (a)-[:HAS_SETTING]->(s)
```

---

#### `Process  -[:INVOLVES]-> Asset`

Un Process coinvolge (utilizza) uno o più Asset.

```cypher
MATCH (p:Process {Name: "NomeProcesso"}), (a:Asset {Name: "NomeAsset"})
MERGE (p)-[:INVOLVES]->(a)
```

---

---

#### `Process  -[:INVOLVES]-> Service`

Un Process coinvolge (utilizza) uno o più Service.

```cypher
MATCH (p:Process {Name: "NomeProcesso"}), (s:Service {Name: "NomeServizio"})
MERGE (p)-[:INVOLVES]->(s)
```

---

---

#### `Process -[:HAS_SETTING]-> Setting`

Un Process è associato a un Setting (es. link a documentazione).

```cypher
MATCH (p:Process {Name: "NomeProcesso"}), (st:Setting {Name: "NomeSetting"})
MERGE (p)-[:HAS_SETTING]->(st)
```

---

---

#### `Division  -[:OWNS]-> Process`

Una Division è proprietaria (responsabile) di un Process.

```cypher
MATCH (d:Division {Name: "NomeDivisione"}), (p:Process {Name: "NomeProcesso"})
MERGE (d)-[:OWNS]->(p)
```

---

---

#### `Division  -[:USES]-> Process`

Una Division utilizza un Process senza esserne proprietaria.

```cypher
MATCH (d:Division {Name: "NomeDivisione"}), (p:Process {Name: "NomeProcesso"})
MERGE (d)-[:USES]->(p)
```

---

---

#### `Process   -[:CLASSIFIED_AS]-> AcnMacroArea`

Un Process è classificato in una determinata ACN Macro Area.

```cypher
MATCH (p:Process {Name: "NomeProcesso"}), (a:AcnMacroArea {Name: "NomeAcnMacroArea"})
MERGE (p)-[:CLASSIFIED_AS]->(a)
```

---


### Script multi‑relazione

Come per i nodi, è possibile creare più relazioni in un unico script separando i blocchi con `WITH 1 AS dummy`:

```cypher
MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Service {Name: "apiERP"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)

WITH 1 AS dummy

MATCH (a:Asset {Name: "ERP Aziendale"}), (s:Service {Name: "dbERP"})
MERGE (a)-[:COMPOSED_BY {IsCritical: true}]->(s)

WITH 1 AS dummy

MATCH (vm:VirtualMachine {Name: "VMERP01"}), (s:Service {Name: "apiERP"})
MERGE (vm)-[:HOSTS]->(s)
```

> 💡 **Regola generale**:  
> Se uno dei `MATCH` non trova corrispondenza (nodo inesistente), quella singola istruzione viene saltata silenziosamente — le relazioni successive continuano a essere elaborate.  
> Verifica sempre che i nodi esistano prima di creare relazioni.