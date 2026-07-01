// Infrastructure/Neo4j/Neo4jNodeMappers.cs
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Core.Enums;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Neo4j;

public static class Neo4jNodeMappers
{
    public static Asset MapAsset(INode n) => new Asset
    {
        Id = n["Id"].As<string>(),
        Name = n["Name"].As<string>(),
        Type = Enum.Parse<AssetType>(n["Type"].As<string>()),
        Description = n.Properties.ContainsKey("Description") ? n["Description"].As<string>() : null,
        Criticality = Enum.Parse<Criticality>(n["Criticality"].As<string>()),
        Bia = n["Bia"].As<bool>(),
        RpoH = n.Properties.ContainsKey("RpoH") ? n["RpoH"]?.As<int>() : null,
        MtoH = n.Properties.ContainsKey("MtoH") ? n["MtoH"]?.As<int>() : null
    };

    public static Division MapDivision(INode n) => new Division
    {
        Id = n["Id"].As<string>(),
        Name = n["Name"].As<string>()
    };

    public static Service MapService(INode n)
    {
        return new Service
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Category = n.Properties.ContainsKey("Category") ? n["Category"].As<string>() : null,
            Version = n.Properties.ContainsKey("Version") ? n["Version"].As<string>() : null,
            ProtocolPort = n.Properties.ContainsKey("ProtocolPort") ? n["ProtocolPort"].As<string>() : null,
            Env = Enum.Parse<Env>(n["Env"].As<string>(), ignoreCase: true),
            Status = n.Properties.ContainsKey("Status") && !string.IsNullOrWhiteSpace(n["Status"]?.As<string>()) ? Enum.Parse<Status>(n["Status"].As<string>(), ignoreCase: true) : null,
            Description = n.Properties.ContainsKey("Description") ? n["Description"].As<string>() : null
        };
    }

    public static Contract MapContract(INode n)
    {
        return new Contract
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            ContractTypes = n.Properties.ContainsKey("ContractTypes") && n["ContractTypes"] != null
                ? n["ContractTypes"].As<System.Collections.Generic.IList<string>>().Select(x => Enum.Parse<ContractType>(x, ignoreCase: true)).ToList()
                : new System.Collections.Generic.List<ContractType>(),
            Sla = n.Properties.ContainsKey("Sla") ? n["Sla"]?.As<int>() : null,
            ContactEmail = n.Properties.ContainsKey("ContactEmail") ? n["ContactEmail"]?.As<string>() : null,
            ContactPhone = n.Properties.ContainsKey("ContactPhone") ? n["ContactPhone"]?.As<string>() : null,
            StartDate = n.Properties.ContainsKey("StartDate") && n["StartDate"] != null ? DateOnly.Parse(n["StartDate"].As<string>()) : null,
            EndDate = n.Properties.ContainsKey("EndDate") && n["EndDate"] != null ? DateOnly.Parse(n["EndDate"].As<string>()) : null,
            Notes = n.Properties.ContainsKey("Notes") ? n["Notes"]?.As<string>() : null
        };
    }

    public static Supplier MapSupplier(INode n)
    {
        return new Supplier
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>()
        };
    }

    public static Setting MapSetting(INode n)
    {
        return new Setting
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Links = n.Properties.ContainsKey("Links")
                ? n["Links"].As<List<string>>()
                : [],
            Description = n.Properties.ContainsKey("Description")
                ? n["Description"]?.As<string>()
                : null
        };
    }

    public static LoginType MapLoginType(INode n)
    {
        return new LoginType
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Mfa = n["Mfa"].As<bool>(),
            Protocol = n.Properties.ContainsKey("Protocol") ? n["Protocol"]?.As<string>() : null
        };
    }

    public static CloudProvider MapCloudProvider(INode n)
    {
        return new CloudProvider
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Type = Enum.Parse<CloudProviderType>(n["Type"].As<string>(), ignoreCase: true),
            PortalUrl = n.Properties.ContainsKey("PortalUrl") ? n["PortalUrl"]?.As<string>() : null,
            Account = n.Properties.ContainsKey("Account") ? n["Account"]?.As<string>() : null
        };
    }

    public static VirtualMachine MapVirtualMachine(INode n)
    {
        return new VirtualMachine
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Type = Enum.Parse<VirtualMachineType>(n["Type"].As<string>(), ignoreCase: true),
            Ip = n.Properties.ContainsKey("Ip") ? n["Ip"]?.As<string>() : null,
            Cluster = n.Properties.ContainsKey("Cluster") ? n["Cluster"]?.As<string>() : null,
            Role = n.Properties.ContainsKey("Role") ? n["Role"]?.As<string>() : null
        };
    }

    public static Process MapProcess(INode n)
    {
        return new Process
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            Description = n.Properties.ContainsKey("Description") ? n["Description"]?.As<string>() : null
        };
    }

    public static AcnMacroArea MapAcnMacroArea(INode n)
    {
        return new AcnMacroArea
        {
            Id = n["Id"].As<string>(),
            Name = n["Name"].As<string>(),
            PreAssignedAcnCategory = Enum.Parse<AcnCategoryOfRelevance>(n["PreAssignedAcnCategory"].As<string>(), ignoreCase: true),
            CustomAcnCategory = n.Properties.ContainsKey("CustomAcnCategory") && !string.IsNullOrWhiteSpace(n["CustomAcnCategory"]?.As<string>()) ? Enum.Parse<AcnCategoryOfRelevance>(n["CustomAcnCategory"].As<string>(), ignoreCase: true) : null
        };
    }
}
