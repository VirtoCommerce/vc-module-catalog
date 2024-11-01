using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class ProductConfiguration
{
    public IList<ProductConfigurationSection> ConfigurationSections { get; set; } = [];
}
