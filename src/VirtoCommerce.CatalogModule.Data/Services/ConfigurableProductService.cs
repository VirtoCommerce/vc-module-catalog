using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;
public class ConfigurableProductService : IConfigurableProductService
{
    private readonly IAssociationService _associationService;

    public ConfigurableProductService(IAssociationService associationService)
    {
        _associationService = associationService;
    }

    public async Task<ProductConfiguration> GetProductConfigurationAsync(string productId)
    {
        var associations = await _associationService.GetAssociationsAsync([productId]);

        return new ProductConfiguration
        {
            ConfigurationSections = associations
                .GroupBy(x => x.Type)
                .Select(x => new ProductConfigurationSection
                {
                    Id = $"{productId}-{x.Key}",
                    Name = x.Key,
                    Type = x.FirstOrDefault()?.AssociatedObjectType,
                    Quantity = x.FirstOrDefault()?.Quantity ?? 0,
                    Description = string.Empty,
                    IsRequired = false,
                    ProductIds = x.Select(y => y.AssociatedObjectId).ToList(),
                })
                .ToList(),
        };
    }
}
