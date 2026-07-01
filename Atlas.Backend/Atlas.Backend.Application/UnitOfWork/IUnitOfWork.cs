using Atlas.Backend.Application.IRepositories.Entities;

namespace Atlas.Backend.Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        IAssetRepository Assets { get; }
        IDivisionRepository Divisions { get; }
        IServiceRepository Services { get; }
        IContractRepository Contracts { get; }
        ISupplierRepository Suppliers { get; }
        ISettingRepository Settings { get; }
        ILoginTypeRepository LoginTypes { get; }
        ICloudProviderRepository CloudProviders { get; }
        IVirtualMachineRepository VirtualMachines { get; }
        IProcessRepository Processes { get; }
        IAcnMacroAreaRepository AcnMacroAreas { get; }
    }
}