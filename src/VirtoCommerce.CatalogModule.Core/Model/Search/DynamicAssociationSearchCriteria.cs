using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class DynamicAssociationSearchCriteria : SearchCriteriaBase
    {
        public string[] StoreIds { get; set; }
        public string Groups { get; set; }
    }
}
