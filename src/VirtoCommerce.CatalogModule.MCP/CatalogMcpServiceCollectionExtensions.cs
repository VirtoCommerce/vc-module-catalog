using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.MCP.Services;

namespace VirtoCommerce.CatalogModule.MCP;

public static class CatalogMcpServiceCollectionExtensions
{
    // The services the catalog manager tools use. The tools themselves are added by AddMcpCapabilities.
    public static IServiceCollection AddCatalogMcpServices(this IServiceCollection services)
    {
        services.AddTransient<CatalogAccess>();
        services.AddTransient<CatalogBackOfficeLinks>();
        services.AddTransient<CatalogReach>();
        services.AddTransient<CatalogTree>();
        services.AddTransient<ProductFindingRows>();
        services.AddTransient<StoreProductIndex>();
        services.AddTransient<StoreProducts>();

        return services;
    }
}
