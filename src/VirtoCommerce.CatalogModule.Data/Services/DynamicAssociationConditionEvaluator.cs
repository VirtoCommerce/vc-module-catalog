using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
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

        public async Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicAssociationConditionEvaluationRequest conditionRequest)
        {
            _requestBuilder
                .AddOutlineSearch(conditionRequest.CategoryIds)
                .AddPropertySearch(conditionRequest.PropertyValues)
                .AddKeywordSearch(conditionRequest.Keyword)
                .AddSortInfo(conditionRequest.Sort)
                .WithPaging(conditionRequest.Skip, conditionRequest.Take);

            var searchResult = await _searchProvider.SearchAsync(KnownDocumentTypes.Product, _requestBuilder.Build());

            return searchResult.Documents.Select(x => x.Id).ToArray();
        }
    }
}
