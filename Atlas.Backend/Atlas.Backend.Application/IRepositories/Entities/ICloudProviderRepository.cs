using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.IRepositories.Entities;

public interface ICloudProviderRepository
{
    Task<CloudProvider?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<CloudProvider?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<CloudProvider>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(CloudProvider cloudProvider, CancellationToken ct = default);
    Task<bool> UpdateAsync(CloudProvider cloudProvider, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
