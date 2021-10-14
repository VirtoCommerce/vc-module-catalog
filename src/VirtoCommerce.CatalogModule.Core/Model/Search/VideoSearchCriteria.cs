using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class VideoSearchCriteria : SearchCriteriaBase
    {
        public IList<string> OwnerIds { get; set; }
        public string OwnerType { get; set; }
    }
}
