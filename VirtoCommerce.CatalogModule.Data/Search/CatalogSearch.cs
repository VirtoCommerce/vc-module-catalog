using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearch
    {
        public IList<string> Ids { get; set; }

        public string SearchPhrase { get; set; }

        public string LanguageCode { get; set; }

        /// <summary>
        /// CategoryId1/CategoryId2, no catalog should be included in the outline
        /// </summary>
        public string Outline { get; set; }

        public string[] Sort { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; } = 20;


        public virtual T AsCriteria<T>(string catalog)
            where T : CatalogSearchCriteria, new()
        {
            var criteria = AbstractTypeFactory<T>.TryCreateInstance();

            criteria.Ids = Ids;
            criteria.SearchPhrase = SearchPhrase;
            criteria.LanguageCode = LanguageCode;
            criteria.Outline = string.Join("/", catalog, Outline).TrimEnd('/', '*').ToLowerInvariant();
            criteria.Skip = Skip;
            criteria.Take = Take;

            return criteria;
        }
    }
}
