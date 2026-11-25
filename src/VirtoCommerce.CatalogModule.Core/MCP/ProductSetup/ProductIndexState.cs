using System;
using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// What the search index holds for a product: the storefront lists, finds and sells only what is in the index.
public class ProductIndexState
{
    // The index has a document for the product.
    public bool IsIndexed { get; set; }

    // The product's change date as the index holds it, when the index has the product.
    public DateTime? ModifiedDate { get; set; }

    // The store's currencies in which the index holds a price for the product.
    public IList<string> PriceCurrencies { get; set; } = [];
}
