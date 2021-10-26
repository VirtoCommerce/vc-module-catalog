using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule2.Core.Model.Search
{
    public class FiltersContainer2 : FiltersContainer
    {
        public override IList<IFilter> GetFiltersExceptSpecified(string excludeFieldName)
        {
            return base.GetFiltersExceptSpecified(excludeFieldName);
        }
    }
}
