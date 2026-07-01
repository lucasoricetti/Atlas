using Atlas.Backend.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Contract?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<Contract>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Contract contract, CancellationToken ct = default);
        Task<bool> UpdateAsync(Contract contract, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}