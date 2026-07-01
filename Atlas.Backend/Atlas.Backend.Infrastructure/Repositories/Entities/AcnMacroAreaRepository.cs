using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class AcnMacroAreaRepository : IAcnMacroAreaRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public AcnMacroAreaRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<AcnMacroArea?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:AcnMacroArea {Id: $id}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapAcnMacroArea(record["s"].As<INode>());
    }

    public async Task<AcnMacroArea?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (s:AcnMacroArea {Name: $name}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapAcnMacroArea(record["s"].As<INode>());
    }

    public async Task<IReadOnlyList<AcnMacroArea>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (s:AcnMacroArea) RETURN s ORDER BY s.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapAcnMacroArea(r["s"].As<INode>()));
    }

    public async Task CreateAsync(AcnMacroArea s, CancellationToken ct = default)
    {
        var query = @"
        CREATE (s:AcnMacroArea {
            Id: $Id,
            Name: $Name,
            PreAssignedAcnCategory: $PreAssignedAcnCategory,
            CustomAcnCategory: $CustomAcnCategory
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(s));
    }

    public async Task<bool> UpdateAsync(AcnMacroArea s, CancellationToken ct = default)
    {
        var query = @"
        MATCH (s:AcnMacroArea {Id: $Id})
        SET s.Name = $Name,
            s.PreAssignedAcnCategory = $PreAssignedAcnCategory,
            s.CustomAcnCategory = $CustomAcnCategory";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(s));
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:AcnMacroArea {Id: $id}) DETACH DELETE s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }

    private static object Serialize(AcnMacroArea s) =>
        new
        {
            s.Id,
            s.Name,
            PreAssignedAcnCategory = s.PreAssignedAcnCategory.ToString(),
            CustomAcnCategory = s.CustomAcnCategory?.ToString()
        };
}
