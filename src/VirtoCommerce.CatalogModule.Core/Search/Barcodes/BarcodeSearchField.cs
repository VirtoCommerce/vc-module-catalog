namespace VirtoCommerce.CatalogModule.Core.Search.Barcodes;

public class BarcodeSearchField
{
    // Index field name: "code", "gtin", "manufacturerPartNumber" or a lowercased catalog property name.
    public string Name { get; set; }

    public string DataType { get; set; }

    public bool IsCollection { get; set; }

    // True for the built-in product fields, false for catalog properties.
    public bool IsProductField { get; set; }
}
