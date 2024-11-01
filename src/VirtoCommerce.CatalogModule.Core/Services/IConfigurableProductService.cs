using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services;
public interface IConfigurableProductService
{
    Task<ProductConfiguration> GetProductConfigurationAsync(string productId);
}
