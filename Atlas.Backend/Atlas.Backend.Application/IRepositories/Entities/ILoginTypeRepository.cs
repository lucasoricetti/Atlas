using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.IRepositories.Entities;

public interface ILoginTypeRepository
{
    Task<LoginType?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<LoginType?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<LoginType>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(LoginType loginType, CancellationToken ct = default);
    Task<bool> UpdateAsync(LoginType loginType, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
