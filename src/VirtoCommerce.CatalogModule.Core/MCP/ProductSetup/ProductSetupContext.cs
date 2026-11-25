using System;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// The products of one call as the store sells them, for every part. Everything in it is a copy, never a cached instance.
public class ProductSetupContext
{
    public Store Store { get; set; }

    // The store's catalog, physical or virtual.
    public Catalog StoreCatalog { get; set; }

    // The physical catalogs whose products the store sells (IProductSetupService.GetStoreCatalogsAsync).
    public IList<Catalog> Catalogs { get; set; } = [];

    // The products, with their outlines, properties, images, descriptions and links.
    public IList<CatalogProduct> Products { get; set; } = [];

    // What the search index holds for each product, by product id; null when the index could not be read.
    public IDictionary<string, ProductIndexState> Index { get; set; } = new Dictionary<string, ProductIndexState>(StringComparer.OrdinalIgnoreCase);
}
