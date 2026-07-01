using Atlas.Backend.Core.Entities;

namespace Atlas.Backend.Application.IRepositories.Entities;

public interface IVirtualMachineRepository
{
    Task<VirtualMachine?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<VirtualMachine?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<VirtualMachine>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(VirtualMachine virtualMachine, CancellationToken ct = default);
    Task<bool> UpdateAsync(VirtualMachine virtualMachine, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
