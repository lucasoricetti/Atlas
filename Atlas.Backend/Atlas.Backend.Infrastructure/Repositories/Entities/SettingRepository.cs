using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class SettingRepository : ISettingRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public SettingRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Setting?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Setting {Id: $id}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapSetting(record["s"].As<INode>());
    }

    public async Task<Setting?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (s:Setting {Name: $name}) RETURN s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapSetting(record["s"].As<INode>());
    }

    public async Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (s:Setting) RETURN s ORDER BY s.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapSetting(r["s"].As<INode>()));
    }

    public async Task CreateAsync(Setting setting, CancellationToken ct = default)
    {
        var query = @"
        CREATE (s:Setting {
            Id: $Id,
            Name: $Name,
            Links: $Links,
            Description: $Description
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, new
        {
            setting.Id,
            setting.Name,
            setting.Links,
            setting.Description
        });
    }

    public async Task<bool> UpdateAsync(Setting setting, CancellationToken ct = default)
    {
        var query = @"
        MATCH (s:Setting {Id: $Id})
        SET s.Name = $Name,
            s.Links = $Links,
            s.Description = $Description";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new
        {
            setting.Id,
            setting.Name,
            setting.Links,
            setting.Description
        });
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (s:Setting {Id: $id}) DETACH DELETE s";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }
}
