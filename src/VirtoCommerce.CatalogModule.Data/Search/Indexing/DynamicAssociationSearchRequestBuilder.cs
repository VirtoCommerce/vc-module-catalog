using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class DynamicAssociationSearchRequestBuilder
    {
        private readonly SearchRequest _searchRequest;

        public DynamicAssociationSearchRequestBuilder()
        {
            _searchRequest = AbstractTypeFactory<SearchRequest>.TryCreateInstance();

            _searchRequest.Filter = new AndFilter { ChildFilters = new List<IFilter>(), };
            _searchRequest.Sorting = new List<SortingField> { new SortingField("__sort") };
            _searchRequest.Skip = 0;
            _searchRequest.Take = 20;
        }

        public virtual SearchRequest Build()
        {
            return _searchRequest;
        }

        public virtual DynamicAssociationSearchRequestBuilder AddPropertySearch(IDictionary<string, string[]> propertyValues)
        {
            foreach (var propertyValue in propertyValues)
            {
                ((AndFilter)_searchRequest.Filter).ChildFilters.Add(new TermFilter
                {
                    FieldName = propertyValue.Key,
                    Values = propertyValue.Value
                });
            }

            return this;
        }

        public virtual DynamicAssociationSearchRequestBuilder AddOutlineSearch(ICollection<string> categoryIds)
        {
            ((AndFilter)_searchRequest.Filter).ChildFilters.Add(new WildCardTermFilter
            {
                FieldName = "__outline",
                Value = string.Join(',', categoryIds.Select(x => $"*{x}")),
            });

            return this;
        }

        public virtual DynamicAssociationSearchRequestBuilder WithPaging(int skip, int take)
        {
            _searchRequest.Skip = skip;
            _searchRequest.Take = take;

            return this;
        }
    }
}
