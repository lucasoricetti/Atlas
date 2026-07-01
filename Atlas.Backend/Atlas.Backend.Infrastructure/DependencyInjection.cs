// Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Atlas.Backend.Infrastructure.Neo4j;
using Atlas.Backend.Application.UnitOfWork;
using Atlas.Backend.Application.IRepositories.Entities;
using Atlas.Backend.Application.IRepositories.Relationships;
using Atlas.Backend.Infrastructure.Repositories.Neo4j;
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Neo4jSettings config)
    {
        services.AddSingleton(config);
        services.AddSingleton<IDriver>(_ => Neo4jDriverFactory.Create(config));

        services.AddScoped<IAssetRepository>(sp =>
            new AssetRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IDivisionRepository>(sp =>
            new DivisionRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IServiceRepository>(sp =>
            new ServiceRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IContractRepository>(sp =>
            new ContractRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<ISupplierRepository>(sp =>
            new SupplierRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<ISettingRepository>(sp =>
            new SettingRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<ILoginTypeRepository>(sp =>
            new LoginTypeRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<ICloudProviderRepository>(sp =>
            new CloudProviderRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IVirtualMachineRepository>(sp =>
            new VirtualMachineRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IProcessRepository>(sp =>
            new ProcessRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IAcnMacroAreaRepository>(sp =>
            new AcnMacroAreaRepository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IRelationshipsV2Repository>(sp =>
            new RelationshipsV2Repository(sp.GetRequiredService<IDriver>(), config));
        services.AddScoped<IRelationshipGraphRepository>(sp =>
            new RelationshipGraphRepository(sp.GetRequiredService<IDriver>(), config));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
