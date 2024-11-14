using System;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

public class ProductConfigurationOption : AuditableEntity, ICloneable
{
    public string SectionId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }

    [JsonIgnore]
    public ProductConfigurationSection Section { get; set; }
    [JsonIgnore]
    public CatalogProduct Product { get; set; }

    #region ICloneable members

    public virtual object Clone()
    {
        return MemberwiseClone();
    }
    #endregion
}
