using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

public class ProductConfigurationSection : AuditableEntity, ICloneable, IHasName
{
    public string ConfigurationId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    [JsonIgnore]
    public ProductConfiguration Configuration { get; set; }

    public IList<ProductConfigurationOption> Options { get; set; } = [];

    #region ICloneable members
    public virtual object Clone()
    {
        var result = (ProductConfigurationSection)MemberwiseClone();

        result.Options = Options?.Select(x => x.CloneTyped()).ToList();

        return result;
    }
    #endregion
}
