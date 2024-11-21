using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

public class ProductConfiguration : AuditableEntity, ICloneable
{
    public string ProductId { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore]
    public CatalogProduct Product { get; set; }

    public IList<ProductConfigurationSection> Sections { get; set; }

    #region ICloneable
    public virtual object Clone()
    {
        var result = (ProductConfiguration)MemberwiseClone();

        result.Sections = Sections?.Select(x => x.CloneTyped()).ToList();

        return result;
    }
    #endregion
}
