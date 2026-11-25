using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// Catalog's way of writing the findings of a product (IProductSetupFindings).
public class ProductSetupFindings : IProductSetupFindings
{
    public virtual McpFinding ForProduct(CatalogProduct product, string part, string id, string severity)
    {
        var finding = AbstractTypeFactory<McpFinding>.TryCreateInstance();
        finding.Id = id;
        finding.Category = part;
        finding.Severity = severity;
        finding.Screen = GetScreen(product);
        finding.ObjectType = nameof(CatalogProduct);
        finding.ObjectId = product.Id;
        finding.ObjectName = product.Name;

        return finding;
    }

    // The product in the back office's catalog browser: "Catalog > MFD > Printers > Laser > HP LaserJet Pro".
    protected virtual string GetScreen(CatalogProduct product)
    {
        var names = new List<string> { "Catalog", product.Catalog?.Name ?? product.CatalogId };
        names.AddRange(GetCategoryNames(product));
        names.Add(product.Name ?? product.Code);

        return string.Join(" > ", names.Where(x => !string.IsNullOrEmpty(x)));
    }

    // The names of the categories above the product in its own catalog, from its outline there.
    protected virtual IEnumerable<string> GetCategoryNames(CatalogProduct product)
    {
        var outline = product.Outlines?.FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(product.CatalogId) == true);

        if (outline == null)
        {
            return [];
        }

        // The first item is the catalog and the last one the product itself.
        return outline.Items.Skip(1).Take(outline.Items.Count - 2).Select(x => x.Name);
    }
}
