using Atlas.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IProcessRepository
    {
        Task<Process?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Process?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<Process>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Process process, CancellationToken ct = default);
        Task<bool> UpdateAsync(Process process, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
