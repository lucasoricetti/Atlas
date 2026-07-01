using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.IRepositories.Entities;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Supplier?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(Supplier supplier, CancellationToken ct = default);
    Task<bool> UpdateAsync(Supplier supplier, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
