using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Core.Enums;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class AssetRepository : IAssetRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public AssetRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Asset?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (a:Asset {Id: $id}) RETURN a";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapAsset(record["a"].As<INode>());
    }

    public async Task<Asset?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (a:Asset {Name: $name}) RETURN a";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapAsset(record["a"].As<INode>());
    }

    public async Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (a:Asset) RETURN a ORDER BY a.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapAsset(r["a"].As<INode>()));
    }

    public async Task CreateAsync(Asset a, CancellationToken ct = default)
    {
        var query = @"
        CREATE (a:Asset {
            Id: $Id,
            Name: $Name,
            Type: $Type,
            Description: $Description,
            Criticality: $Criticality,
            Bia: $Bia,
            RpoH: $RpoH,
            MtoH: $MtoH
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(a));
    }

    public async Task<bool> UpdateAsync(Asset a, CancellationToken ct = default)
    {
        var query = @"
        MATCH (a:Asset {Id: $Id})
        SET a.Name = $Name,
            a.Type = $Type,
            a.Description = $Description,
            a.Criticality = $Criticality,
            a.Bia = $Bia,
            a.RpoH = $RpoH,
            a.MtoH = $MtoH";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(a));
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;

    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (a:Asset {Id: $id}) DETACH DELETE a";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;

    }

    private static object Serialize(Asset a) =>
        new
        {
            a.Id,
            a.Name,
            Type = a.Type.ToString(),
            a.Description,
            Criticality = a.Criticality.ToString(),
            a.Bia,
            a.RpoH,
            a.MtoH
        };
}