using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;
public class ConfigurableProductService : IConfigurableProductService
{
    private readonly IAssociationService _associationService;

    public ConfigurableProductService(IAssociationService associationService)
    {
        _associationService = associationService;
    }

    [Obsolete("Use ProductConfigurationService instead")]
    public async Task<ProductConfiguration> GetProductConfigurationAsync(string productId)
    {
        var associations = await _associationService.GetAssociationsAsync([productId]);

        var result = new ProductConfiguration
        {
            Sections = associations
                .GroupBy(x => x.Type)
                .Select(x => new ProductConfigurationSection
                {
                    Id = $"{productId}-{x.Key}",
                    Name = x.Key,
                    Type = x.FirstOrDefault()?.AssociatedObjectType,
                    Description = string.Empty,
                    IsRequired = false,
                    Options = x.Select(y => new ProductConfigurationOption
                    {
                        Id = y.AssociatedObjectId,
                        ProductId = y.AssociatedObjectId,
                        Quantity = y.Quantity ?? 1,
                    }).ToList(),
                })
                .ToList(),
        };

        return result;
    }
}
