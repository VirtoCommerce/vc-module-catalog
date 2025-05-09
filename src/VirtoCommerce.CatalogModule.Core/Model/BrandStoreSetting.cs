using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class BrandStoreSetting : AuditableEntity, ICloneable
{
    public string StoreId { get; set; }

    public string BrandPropertyId { get; set; }

    public string BrandCatalogId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
