using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search;

public class ProductConfigurationSearchCriteria : SearchCriteriaBase
{
    public string ProductId { get; set; }
    public bool? IsActive { get; set; }
}
