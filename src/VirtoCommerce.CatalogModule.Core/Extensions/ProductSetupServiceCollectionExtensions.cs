using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class ProductSetupServiceCollectionExtensions
{
    // Adds a module's part of a product as a store sells it. A module that is not installed registers nothing, so it has
    // no part. A part's name is its key: a part registered later with the same name replaces it, which is how a module
    // overrides another module's part with a subclass.
    public static IServiceCollection AddProductSetupPart<TPart>(this IServiceCollection services)
        where TPart : class, IProductSetupPart
    {
        services.AddTransient<IProductSetupPart, TPart>();
        return services;
    }
}
