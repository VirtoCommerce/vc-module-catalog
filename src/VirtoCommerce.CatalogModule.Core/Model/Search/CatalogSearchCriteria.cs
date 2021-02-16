using System;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class CatalogSearchCriteria : SearchCriteriaBase
    {
        public string[] CatalogIds
        {
            get => ObjectIds?.ToArray() ?? Array.Empty<string>();
            set => ObjectIds = value;
        }
    }
}
