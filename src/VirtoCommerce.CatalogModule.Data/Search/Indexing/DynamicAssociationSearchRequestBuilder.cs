using System.Collections.Generic;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class DynamicAssociationSearchRequestBuilder
    {
        private readonly SearchRequest _searchRequest;

        public DynamicAssociationSearchRequestBuilder()
        {
            _searchRequest = new SearchRequest
            {
                Filter = new AndFilter
                {
                    ChildFilters = new List<IFilter>(),
                },
                SearchFields = new List<string> { "__content" },
                Sorting = new List<SortingField> { new SortingField("__sort") },
                Skip = 0,
                Take = 20,
            };
        }
    }
}
