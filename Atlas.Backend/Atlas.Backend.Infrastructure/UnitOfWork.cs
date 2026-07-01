// Infrastructure/UnitOfWork.cs
using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Application.UnitOfWork;

namespace Atlas.Backend.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    public IAssetRepository Assets { get; }
    public IDivisionRepository Divisions { get; }
    public IServiceRepository Services { get; }
    public IContractRepository Contracts { get; }
    public ISupplierRepository Suppliers { get; }
    public ISettingRepository Settings { get; }
    public ILoginTypeRepository LoginTypes { get; }
    public ICloudProviderRepository CloudProviders { get; }
    public IVirtualMachineRepository VirtualMachines { get; }
    public IProcessRepository Processes { get; }
    public IAcnMacroAreaRepository AcnMacroAreas { get; }

    public UnitOfWork(
        IAssetRepository assets,
        IDivisionRepository divisions,
        IServiceRepository services,
        IContractRepository contracts,
        ISupplierRepository suppliers,
        ISettingRepository settings,
        ILoginTypeRepository loginTypes,
        ICloudProviderRepository cloudProviders,
        IVirtualMachineRepository virtualMachines,
        IProcessRepository processes,
        IAcnMacroAreaRepository acnMacroAreas)
    {
        Assets = assets;
        Divisions = divisions;
        Services = services;
        Contracts = contracts;
        Suppliers = suppliers;
        Settings = settings;
        LoginTypes = loginTypes;
        CloudProviders = cloudProviders;
        VirtualMachines = virtualMachines;
        Processes = processes;
        AcnMacroAreas = acnMacroAreas;
    }
}