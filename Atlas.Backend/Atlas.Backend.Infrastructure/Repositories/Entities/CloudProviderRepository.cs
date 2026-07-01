using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class CloudProviderRepository : ICloudProviderRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public CloudProviderRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<CloudProvider?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (cp:CloudProvider {Id: $id}) RETURN cp";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapCloudProvider(record["cp"].As<INode>());
    }

    public async Task<CloudProvider?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (cp:CloudProvider {Name: $name}) RETURN cp";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapCloudProvider(record["cp"].As<INode>());
    }

    public async Task<IReadOnlyList<CloudProvider>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (cp:CloudProvider) RETURN cp ORDER BY cp.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapCloudProvider(r["cp"].As<INode>()));
    }

    public async Task CreateAsync(CloudProvider cp, CancellationToken ct = default)
    {
        var query = @"
        CREATE (cp:CloudProvider {
            Id: $Id,
            Name: $Name,
            Type: $Type,
            PortalUrl: $PortalUrl,
            Account: $Account
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(cp));
    }

    public async Task<bool> UpdateAsync(CloudProvider cp, CancellationToken ct = default)
    {
        var query = @"
        MATCH (cp:CloudProvider {Id: $Id})
        SET cp.Name = $Name,
            cp.Type = $Type,
            cp.PortalUrl = $PortalUrl,
            cp.Account = $Account
        RETURN 1 AS updated";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(cp));
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (cp:CloudProvider {Id: $id}) DELETE cp RETURN 1 AS deleted";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    private static Dictionary<string, object?> Serialize(CloudProvider cp) => new()
    {
        ["Id"] = cp.Id,
        ["Name"] = cp.Name,
        ["Type"] = cp.Type.ToString(),
        ["PortalUrl"] = cp.PortalUrl,
        ["Account"] = cp.Account
    };
}
