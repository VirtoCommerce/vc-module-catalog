using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationConditionEvaluator : IDynamicAssociationConditionEvaluator
    {
        private readonly DynamicAssociationSearchRequestBuilder _requestBuilder;
        private readonly ISearchProvider _searchProvider;

        public DynamicAssociationConditionEvaluator(
            DynamicAssociationSearchRequestBuilder requestBuilder,
            ISearchProvider searchProvider
            )
        {
            _requestBuilder = requestBuilder;
            _searchProvider = searchProvider;
        }

        public async Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicAssociationsRuleEvaluationContext searchContext, DynamicAssociationCondition dynamicAssociationCondition)
        {
            _requestBuilder
                .AddOutlineSearch(dynamicAssociationCondition.CategoryIds)
                .AddPropertySearch(dynamicAssociationCondition.PropertyValues)
                .WithPaging(searchContext.Skip, searchContext.Take);

            var searchResult = await _searchProvider.SearchAsync(KnownDocumentTypes.Product, _requestBuilder.Build());

            return searchResult.Documents.Select(x => x.Id).ToArray();
        }
    }
}
