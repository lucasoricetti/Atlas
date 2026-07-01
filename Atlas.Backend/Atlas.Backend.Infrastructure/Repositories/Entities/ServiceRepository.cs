using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class ServiceRepository : IServiceRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public ServiceRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Service?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Service {Id: $id}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapService(record["s"].As<INode>());
    }

    public async Task<Service?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (s:Service {Name: $name}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapService(record["s"].As<INode>());
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (s:Service) RETURN s ORDER BY s.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapService(r["s"].As<INode>()));
    }

    public async Task CreateAsync(Service s, CancellationToken ct = default)
    {
        var query = @"
        CREATE (s:Service {
            Id: $Id,
            Name: $Name,
            Category: $Category,
            Version: $Version,
            ProtocolPort: $ProtocolPort,
            Env: $Env,
            Status: $Status,
            Description: $Description
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(s));
    }

    public async Task<bool> UpdateAsync(Service s, CancellationToken ct = default)
    {
        var query = @"
        MATCH (s:Service {Id: $Id})
        SET s.Name = $Name,
            s.Category = $Category,
            s.Version = $Version,
            s.ProtocolPort = $ProtocolPort,
            s.Env = $Env,
            s.Status = $Status,
            s.Description = $Description";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(s));
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Service {Id: $id}) DETACH DELETE s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }

    private static object Serialize(Service s) =>
        new
        {
            s.Id,
            s.Name,
            s.Category,
            s.Version,
            s.ProtocolPort,
            Env = s.Env.ToString(),
            Status = s.Status?.ToString(),
            s.Description
        };
}
