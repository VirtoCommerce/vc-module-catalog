using System.Collections.Generic;
using System.ComponentModel;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// Catalog's fields of a product row, as get_product shows a product and the product tools take one: the other parts'
// sections sit beside them under their names, such as "price" and "stock". A field that is left out changes nothing.
public class CatalogProductSection
{
    [Description("SKU: the key of every product action, unique in the product's physical catalog.")]
    public string Sku { get; set; }

    [Description("Product name.")]
    public string Name { get; set; }

    [Description("Category: its path as get_catalog_structure shows it, such as 'Printers > Laser', or its id.")]
    public string Category { get; set; }

    [Description("SKU of the product this one is a variation of, in the same catalog; null for a product.")]
    public string ParentSku { get; set; }

    [Description("Description in the store's default language.")]
    public string Description { get; set; }

    [Description("Descriptions in the store's other languages, by language code, such as de-DE.")]
    public IDictionary<string, string> Descriptions { get; set; }

    [Description("Property values by property name: a text, a number, true or false, a date, or a list for a property " +
        "with several values. A dictionary property takes one of its values.")]
    public IDictionary<string, object> Properties { get; set; }

    [Description("Whether customers see the product: false hides it from the storefront.")]
    public bool? Active { get; set; }

    [Description("Whether customers can buy the product: false lists it, but it cannot be bought.")]
    public bool? Buyable { get; set; }

    [Description("Whether the product's stock is tracked: true sells it only while a center of the store has it in stock.")]
    public bool? TrackInventory { get; set; }

    [Description("Global trade item number, such as an EAN or a UPC.")]
    public string Gtin { get; set; }

    [Description("Vendor.")]
    public string Vendor { get; set; }

    [Description("Manufacturer part number.")]
    public string ManufacturerPartNumber { get; set; }
}
