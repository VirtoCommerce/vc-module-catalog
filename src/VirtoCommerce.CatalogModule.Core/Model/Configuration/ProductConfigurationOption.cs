using System;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

public class ProductConfigurationOption : AuditableEntity, ICloneable
{
    public string SectionId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    [JsonIgnore]
    public ProductConfigurationSection Section { get; set; }

    [JsonIgnore]
    public CatalogProduct Product { get; set; }

    public virtual string ProductName => Product?.Name;

    public virtual string ProductImageUrl => Product?.ImgSrc;

    public virtual string ProductType => Product?.ProductType;

    #region ICloneable members
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
    #endregion
}
