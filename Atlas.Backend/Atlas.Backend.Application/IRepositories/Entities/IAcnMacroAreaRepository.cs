using Atlas.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Application.IRepositories.Entities
{
    public interface IAcnMacroAreaRepository
    {
        Task<AcnMacroArea?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<AcnMacroArea?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<IReadOnlyList<AcnMacroArea>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(AcnMacroArea acnMacroArea, CancellationToken ct = default);
        Task<bool> UpdateAsync(AcnMacroArea acnMacroArea, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
