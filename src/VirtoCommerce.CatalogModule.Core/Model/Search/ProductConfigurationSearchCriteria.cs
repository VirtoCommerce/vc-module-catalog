using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search;

public class ProductConfigurationSearchCriteria : SearchCriteriaBase
{
    public string ProductId { get; set; }

    private IList<string> _productIds;
    public IList<string> ProductIds
    {
        get
        {
            if (_productIds == null && !string.IsNullOrEmpty(ProductId))
            {
                _productIds = [ProductId];
            }
            return _productIds;
        }
        set
        {
            _productIds = value;
        }
    }

    public bool? IsActive { get; set; }
}
