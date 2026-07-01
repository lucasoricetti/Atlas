using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Core.Entities;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly IDriver _driver;
        private readonly string _db;

        public ProcessRepository(IDriver driver, Neo4jSettings settings)
        {
            _driver = driver;
            _db = settings.Database;
        }

        public async Task<Process?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var query = "MATCH (p:Process {Id: $id}) RETURN p";

            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            var result = await session.RunAsync(query, new { id });

            var record = await result.SingleOrDefaultAsync();
            return record == null ? null : Neo4jNodeMappers.MapProcess(record["p"].As<INode>());
        }

        public async Task<Process?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            var query = "MATCH (p:Process {Name: $name}) RETURN p";

            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            var result = await session.RunAsync(query, new { name });

            var record = await result.SingleOrDefaultAsync();
            return record == null ? null : Neo4jNodeMappers.MapProcess(record["p"].As<INode>());
        }

        public async Task<IReadOnlyList<Process>> GetAllAsync(CancellationToken ct = default)
        {
            var query = "MATCH (p:Process) RETURN p ORDER BY p.Name";

            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            var result = await session.RunAsync(query);

            return await result.ToListAsync(r => Neo4jNodeMappers.MapProcess(r["p"].As<INode>()));
        }

        public async Task CreateAsync(Process p, CancellationToken ct = default)
        {
            var query = @"
                CREATE (p:Process {
                    Id: $Id,
                    Name: $Name,
                    Description: $Description
                })";

            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            await session.RunAsync(query, p);
        }

        public async Task<bool> UpdateAsync(Process p, CancellationToken ct = default)
        {
            var query = @"
                MATCH (p:Process {Id: $Id})
                SET p.Name = $Name,
                    p.Description = $Description";
                    
            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            var result = await session.RunAsync(query, p);
            var summary = await result.ConsumeAsync();
            return summary.Counters.PropertiesSet > 0;

        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            var query = "MATCH (p:Process {Id: $id}) DETACH DELETE a";

            using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
            var result = await session.RunAsync(query, new { id });
            var summary = await result.ConsumeAsync();
            return summary.Counters.NodesDeleted > 0;
        }
    }
}