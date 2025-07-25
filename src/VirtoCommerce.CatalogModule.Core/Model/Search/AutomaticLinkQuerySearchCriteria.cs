using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search;

public class AutomaticLinkQuerySearchCriteria : SearchCriteriaBase
{
    public string TargetCategoryId { get; set; }
}
