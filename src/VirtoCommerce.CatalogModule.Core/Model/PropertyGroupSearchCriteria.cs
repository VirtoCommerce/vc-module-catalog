using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class PropertyGroupSearchCriteria : SearchCriteriaBase, IHasCatalogId
    {
        public string CatalogId { get; set; }
    }
}
