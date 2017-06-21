using System.Collections.Generic;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearchCriteria : SearchCriteriaBase
    {
        public IList<string> Ids { get; set; }

        public string SearchPhrase { get; set; }

        public string LanguageCode { get; set; }

        public string StoreId { get; set; }

        public string CatalogId { get; set; }

        /// <summary>
        /// CategoryId1/CategoryId2, no catalog should be included in the outline
        /// </summary>
        public string Outline { get; set; }
    }
}
