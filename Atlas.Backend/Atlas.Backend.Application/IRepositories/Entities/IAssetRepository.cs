using Atlas.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IAssetRepository
    {
        Task<Asset?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Asset?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Asset asset, CancellationToken ct = default);
        Task<bool> UpdateAsync(Asset asset, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
