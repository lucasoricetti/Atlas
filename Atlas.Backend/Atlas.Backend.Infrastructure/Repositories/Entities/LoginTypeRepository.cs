using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class LoginTypeRepository : ILoginTypeRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public LoginTypeRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<LoginType?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (lt:LoginType {Id: $id}) RETURN lt";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapLoginType(record["lt"].As<INode>());
    }

    public async Task<LoginType?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (lt:LoginType {Name: $name}) RETURN lt";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapLoginType(record["lt"].As<INode>());
    }

    public async Task<IReadOnlyList<LoginType>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (lt:LoginType) RETURN lt ORDER BY lt.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapLoginType(r["lt"].As<INode>()));
    }

    public async Task CreateAsync(LoginType lt, CancellationToken ct = default)
    {
        var query = @"
        CREATE (lt:LoginType {
            Id: $Id,
            Name: $Name,
            Mfa: $Mfa,
            Protocol: $Protocol
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(lt));
    }

    public async Task<bool> UpdateAsync(LoginType lt, CancellationToken ct = default)
    {
        var query = @"
        MATCH (lt:LoginType {Id: $Id})
        SET lt.Name = $Name,
            lt.Mfa = $Mfa,
            lt.Protocol = $Protocol
        RETURN 1 AS updated";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(lt));
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (lt:LoginType {Id: $id}) DELETE lt RETURN 1 AS deleted";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    private static Dictionary<string, object?> Serialize(LoginType lt) => new()
    {
        ["Id"] = lt.Id,
        ["Name"] = lt.Name,
        ["Mfa"] = lt.Mfa,
        ["Protocol"] = lt.Protocol
    };
}
