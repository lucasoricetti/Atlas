using Atlas.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Service?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Service service, CancellationToken ct = default);
        Task<bool> UpdateAsync(Service service, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
