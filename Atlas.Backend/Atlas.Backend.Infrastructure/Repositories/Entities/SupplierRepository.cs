using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class SupplierRepository : ISupplierRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public SupplierRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Supplier?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Supplier {Id: $id}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapSupplier(record["s"].As<INode>());
    }

    public async Task<Supplier?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (s:Supplier {Name: $name}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapSupplier(record["s"].As<INode>());
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (s:Supplier) RETURN s ORDER BY s.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapSupplier(r["s"].As<INode>()));
    }

    public async Task CreateAsync(Supplier supplier, CancellationToken ct = default)
    {
        var query = @"
        CREATE (s:Supplier {
            Id: $Id,
            Name: $Name
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, supplier);
    }

    public async Task<bool> UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        var query = @"
        MATCH (s:Supplier {Id: $Id})
        SET s.Name = $Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, supplier);
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Supplier {Id: $id}) DETACH DELETE s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }
}
