using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class VirtualMachineRepository : IVirtualMachineRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public VirtualMachineRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<VirtualMachine?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (vm:VirtualMachine {Id: $id}) RETURN vm";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapVirtualMachine(record["vm"].As<INode>());
    }

    public async Task<VirtualMachine?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (vm:VirtualMachine {Name: $name}) RETURN vm";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapVirtualMachine(record["vm"].As<INode>());
    }

    public async Task<IReadOnlyList<VirtualMachine>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (vm:VirtualMachine) RETURN vm ORDER BY vm.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapVirtualMachine(r["vm"].As<INode>()));
    }

    public async Task CreateAsync(VirtualMachine vm, CancellationToken ct = default)
    {
        var query = @"
        CREATE (vm:VirtualMachine {
            Id: $Id,
            Name: $Name,
            Type: $Type,
            Ip: $Ip,
            Cluster: $Cluster,
            Role: $Role
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(vm));
    }

    public async Task<bool> UpdateAsync(VirtualMachine vm, CancellationToken ct = default)
    {
        var query = @"
        MATCH (vm:VirtualMachine {Id: $Id})
        SET vm.Name = $Name,
            vm.Type = $Type,
            vm.Ip = $Ip,
            vm.Cluster = $Cluster,
            vm.Role = $Role
        RETURN 1 AS updated";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(vm));
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (vm:VirtualMachine {Id: $id}) DELETE vm RETURN 1 AS deleted";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    private static Dictionary<string, object?> Serialize(VirtualMachine vm) => new()
    {
        ["Id"] = vm.Id,
        ["Name"] = vm.Name,
        ["Type"] = vm.Type.ToString(),
        ["Ip"] = vm.Ip,
        ["Cluster"] = vm.Cluster,
        ["Role"] = vm.Role
    };
}
