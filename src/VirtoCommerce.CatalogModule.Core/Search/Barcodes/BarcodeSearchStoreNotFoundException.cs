using System;

namespace VirtoCommerce.CatalogModule.Core.Search.Barcodes;

public class BarcodeSearchStoreNotFoundException : InvalidOperationException
{
    public BarcodeSearchStoreNotFoundException(string storeId)
        : base($"Store '{storeId}' was not found.")
    {
    }
}
