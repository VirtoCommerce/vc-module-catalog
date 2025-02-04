using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class PropertySearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }

        private string[] _catalogIds;
        public string[] CatalogIds
        {
            get
            {
                if (_catalogIds == null && !string.IsNullOrEmpty(CatalogId))
                {
                    _catalogIds = new[] { CatalogId };
                }
                return _catalogIds;
            }
            set
            {
                _catalogIds = value;
            }
        }

        public IList<string> PropertyNames { get; set; }
        public IList<string> PropertyTypes { get; set; }

        public IList<PropertyValueType> PropertyValueTypes { get; set; }

        public IList<PropertyValueType> ExcludedPropertyValueTypes { get; set; }
    }
}
