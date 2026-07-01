using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

public class ContractRepository : IContractRepository
{
    private readonly IDriver _driver;
    private readonly string _db;

    public ContractRepository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public async Task<Contract?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (c:Contract {Id: $id}) RETURN c";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapContract(record["c"].As<INode>());
    }

    public async Task<Contract?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var query = "MATCH (c:Contract {Name: $name}) RETURN c";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { name });

        var record = await result.SingleOrDefaultAsync();
        return record == null ? null : Neo4jNodeMappers.MapContract(record["c"].As<INode>());
    }

    public async Task<IReadOnlyList<Contract>> GetAllAsync(CancellationToken ct = default)
    {
        var query = "MATCH (c:Contract) RETURN c ORDER BY c.Name";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query);

        return await result.ToListAsync(r => Neo4jNodeMappers.MapContract(r["c"].As<INode>()));
    }

    public async Task CreateAsync(Contract c, CancellationToken ct = default)
    {
        var query = @"
        CREATE (c:Contract {
            Id: $Id,
            Name: $Name,
            ContractTypes: $ContractTypes,
            Sla: $Sla,
            ContactEmail: $ContactEmail,
            ContactPhone: $ContactPhone,
            StartDate: $StartDate,
            EndDate: $EndDate,
            Notes: $Notes
        })";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        await session.RunAsync(query, Serialize(c));
    }

    public async Task<bool> UpdateAsync(Contract c, CancellationToken ct = default)
    {
        var query = @"
        MATCH (c:Contract {Id: $Id})
        SET c.Name = $Name,
            c.ContractTypes = $ContractTypes,
            c.Sla = $Sla,
            c.ContactEmail = $ContactEmail,
            c.ContactPhone = $ContactPhone,
            c.StartDate = $StartDate,
            c.EndDate = $EndDate,
            c.Notes = $Notes";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, Serialize(c));
        var summary = await result.ConsumeAsync();
        return summary.Counters.PropertiesSet > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var query = "MATCH (c:Contract {Id: $id}) DETACH DELETE c";

        using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.RunAsync(query, new { id });
        var summary = await result.ConsumeAsync();
        return summary.Counters.NodesDeleted > 0;
    }

    private static object Serialize(Contract c) =>
        new
        {
            c.Id,
            c.Name,
            ContractTypes = c.ContractTypes.Select(x => x.ToString()).ToList(),
            c.Sla,
            c.ContactEmail,
            c.ContactPhone,
            StartDate = c.StartDate?.ToString("yyyy-MM-dd"),
            EndDate = c.EndDate?.ToString("yyyy-MM-dd"),
            c.Notes
        };
}
