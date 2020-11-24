using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public interface IHasExcludedProperties
    {
        IList<ExcludedProperty> ExcludedProperties { get; set; }
    }
}
