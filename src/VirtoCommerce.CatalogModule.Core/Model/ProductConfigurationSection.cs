using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class ProductConfigurationSection
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public IList<ProductConfigurationOption> Options { get; set; } = [];
}

public class ProductConfigurationOption
{
    public string Id { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
