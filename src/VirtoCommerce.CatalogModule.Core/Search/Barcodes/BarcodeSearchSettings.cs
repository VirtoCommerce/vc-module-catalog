using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Search.Barcodes;

public class BarcodeSearchSettings
{
    public bool ScannerEnabled { get; set; } = true;

    // Product index field names (lowercase for catalog properties). Empty = match a scanned code by full-text search.
    public IList<string> Fields { get; set; } = [];
}
