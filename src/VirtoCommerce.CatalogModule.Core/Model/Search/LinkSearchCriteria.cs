using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class LinkSearchCriteria : SearchCriteriaBase
    {
        public IEnumerable<string> CategoryIds { get; set; } = new List<string>();

        public IEnumerable<string> CatalogIds { get; set; } = new List<string>();
    }
}
