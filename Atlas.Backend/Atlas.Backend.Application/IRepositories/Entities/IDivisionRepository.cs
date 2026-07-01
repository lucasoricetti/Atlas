using Atlas.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IDivisionRepository
    {
        Task<Division?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Division?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<Division>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Division d, CancellationToken ct = default);
        Task<bool> UpdateAsync(Division d, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
