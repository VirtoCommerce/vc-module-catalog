using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class CatalogSearchCriteria : SearchCriteriaBase
    {
        public string[] CatalogIds { get; set; }

        public string[] OuterIds { get; set; }

        public bool? IsVirtual { get; set; }
    }
}
