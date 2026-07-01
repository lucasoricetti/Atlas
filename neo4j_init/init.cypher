// Definizione della primary key 
CREATE CONSTRAINT division_id_pk IF NOT EXISTS 
FOR (d:Division) 
REQUIRE d.Id IS UNIQUE; 
CREATE CONSTRAINT division_name_unique IF NOT EXISTS 
FOR (d:Division) 
REQUIRE d.Name IS UNIQUE; 

// Definizione della primary key 
CREATE CONSTRAINT asset_id_pk IF NOT EXISTS 
FOR (a:Asset) 
REQUIRE a.Id IS UNIQUE; 
CREATE CONSTRAINT asset_name_unique IF NOT EXISTS 
FOR (a:Asset) 
REQUIRE a.Name IS UNIQUE; 

// Definizione della primary key  
CREATE CONSTRAINT service_id_pk IF NOT EXISTS  
FOR (s:Service)  
REQUIRE s.Id IS UNIQUE;  
CREATE CONSTRAINT service_name_unique IF NOT EXISTS  
FOR (s:Service)  
REQUIRE s.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT contract_id_pk IF NOT EXISTS  
FOR (c:Contract)  
REQUIRE c.Id IS UNIQUE; 
CREATE CONSTRAINT contract_name_unique IF NOT EXISTS  
FOR (c:Contract)  
REQUIRE c.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT supplier_id_pk IF NOT EXISTS  
FOR (s:Supplier)  
REQUIRE s.Id IS UNIQUE; 
CREATE CONSTRAINT supplier_name_unique IF NOT EXISTS  
FOR (s:Supplier)  
REQUIRE s.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT setting_id_pk IF NOT EXISTS  
FOR (s:Setting)  
REQUIRE s.Id IS UNIQUE; 
CREATE CONSTRAINT setting_name_unique IF NOT EXISTS  
FOR (s:Setting)  
REQUIRE s.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT login_type_id_pk IF NOT EXISTS  
FOR (l:LoginType)  
REQUIRE l.Id IS UNIQUE;  
CREATE CONSTRAINT login_type_name_unique IF NOT EXISTS  
FOR (l:LoginType)  
REQUIRE l.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT cloud_provider_id_pk IF NOT EXISTS  
FOR (c:CloudProvider)  
REQUIRE c.Id IS UNIQUE;  
CREATE CONSTRAINT cloud_provider_name_unique IF NOT EXISTS  
FOR (c:CloudProvider)  
REQUIRE c.Name IS UNIQUE;  

// Definizione della primary key  
CREATE CONSTRAINT virtual_machine_id_pk IF NOT EXISTS  
FOR (v:VirtualMachine)  
REQUIRE v.Id IS UNIQUE;  
CREATE CONSTRAINT virtual_machine_name_unique IF NOT EXISTS  
FOR (v:VirtualMachine)  
REQUIRE v.Name IS UNIQUE;

// Definizione della primary key  
CREATE CONSTRAINT process_id_pk IF NOT EXISTS  
FOR (p:Process)  
REQUIRE p.Id IS UNIQUE;  
CREATE CONSTRAINT process_name_unique IF NOT EXISTS  
FOR (p:Process)  
REQUIRE p.Name IS UNIQUE;

// Definizione della primary key  
CREATE CONSTRAINT acn_macro_area_id_pk IF NOT EXISTS  
FOR (a:AcnMacroArea)  
REQUIRE a.Id IS UNIQUE;  
CREATE CONSTRAINT acn_macro_area_name_unique IF NOT EXISTS  
FOR (a:AcnMacroArea)  
REQUIRE a.Name IS UNIQUE;