using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

public class ProductConfigurationOption : AuditableEntity, ICloneable
{
    public string ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    #region ICloneable members
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
    #endregion
}
