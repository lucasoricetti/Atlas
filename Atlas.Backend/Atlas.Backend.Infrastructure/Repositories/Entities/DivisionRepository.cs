using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class DivisionRepository : IDivisionRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public DivisionRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Division?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (d:Division {Id: $id}) RETURN d";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapDivision(record["d"].As<INode>());
    }

    public async Task<Division?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (d:Division {Name: $name}) RETURN d";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapDivision(record["d"].As<INode>());
    }

    public async Task<IReadOnlyList<Division>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (d:Division) RETURN d ORDER BY d.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapDivision(r["d"].As<INode>()));
    }

    public async Task CreateAsync(Division d, CancellationToken ct = default)
    {
        var query = @"
        CREATE (d:Division {
            Id: $Id,
            Name: $Name
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, d);
    }

    public async Task<bool> UpdateAsync(Division d, CancellationToken ct = default)
    {
        var query = @"
        MATCH (d:Division {Id: $Id})
        SET d.Name = $Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, d);
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (d:Division {Id: $id}) DETACH DELETE d";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }
}