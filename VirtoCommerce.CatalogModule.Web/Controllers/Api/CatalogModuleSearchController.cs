using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Security;
using SearchCriteria = VirtoCommerce.CatalogModule.Web.Model.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/search")]
    public class CatalogModuleSearchController : CatalogBaseController
    {
        private readonly ICatalogSearchService _searchService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IProductSearchService _productSearchService;
        private readonly ICategorySearchService _categorySearchService;

        public CatalogModuleSearchController(ISecurityService securityService, IPermissionScopeService permissionScopeService, ICatalogSearchService searchService, IBlobUrlResolver blobUrlResolver, IProductSearchService productSearchService, ICategorySearchService categorySearchService)
            : base(securityService, permissionScopeService)
        {
            _searchService = searchService;
            _blobUrlResolver = blobUrlResolver;
            _productSearchService = productSearchService;
            _categorySearchService = categorySearchService;
        }


        /// <summary>
        /// Searches for the items by complex criteria
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CatalogSearchResult))]
        public IHttpActionResult Search(SearchCriteria criteria)
        {
            var coreModelCriteria = criteria.ToCoreModel();
            ApplyRestrictionsForCurrentUser(coreModelCriteria);
            var serviceResult = _searchService.Search(coreModelCriteria);

            return Ok(serviceResult.ToWebModel(_blobUrlResolver));
        }

        [HttpPost]
        [Route("{storeId}/products")]
        [ResponseType(typeof(ProductSearchResult))]
        public async Task<IHttpActionResult> SearchProducts(string storeId, ProductSearch criteria)
        {
            var result = await _productSearchService.SearchAsync(storeId, criteria);
            return Ok(result);
        }

        [HttpPost]
        [Route("{storeId}/categories")]
        [ResponseType(typeof(CategorySearchResult))]
        public async Task<IHttpActionResult> SearchCategories(string storeId, CategorySearch criteria)
        {
            var result = await _categorySearchService.SearchAsync(storeId, criteria);
            return Ok(result);
        }
    }
}
