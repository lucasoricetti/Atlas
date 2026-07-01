using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.IRepositories.Entities;

public interface ISettingRepository
{
    Task<Setting?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Setting?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(Setting setting, CancellationToken ct = default);
    Task<bool> UpdateAsync(Setting setting, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
