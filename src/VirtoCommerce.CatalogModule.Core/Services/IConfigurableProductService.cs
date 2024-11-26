using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;

namespace VirtoCommerce.CatalogModule.Core.Services;
public interface IConfigurableProductService
{
    [Obsolete("Use IProductConfigurationSearchService.SearchAsync() instead")]
    Task<ProductConfiguration> GetProductConfigurationAsync(string productId);
}
