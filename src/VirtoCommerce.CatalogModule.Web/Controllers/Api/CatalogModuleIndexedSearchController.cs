using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/search")]
    [Authorize]
    public class CatalogModuleIndexedSearchController : Controller
    {
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        public CatalogModuleIndexedSearchController(
            IProductIndexedSearchService productIndexedSearchService
            , ICategoryIndexedSearchService categoryIndexedSearchService
            , IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            _productIndexedSearchService = productIndexedSearchService;
            _categoryIndexedSearchService = categoryIndexedSearchService;
            _jsonOptions = jsonOptions.Value;
        }

        [HttpPost]
        [Route("products")]
        public async Task<ActionResult<ProductIndexedSearchResult>> SearchProducts([FromBody] ProductIndexedSearchCriteria criteria)
        {
            criteria.ObjectType = KnownDocumentTypes.Product;
            var result = await _productIndexedSearchService.SearchAsync(criteria);

            //It is a important to return serialized data by such way. Instead you have a slow response time for large outputs 
            //https://github.com/dotnet/aspnetcore/issues/19646
            return Content(JsonConvert.SerializeObject(result, _jsonOptions.SerializerSettings), "application/json");
        }

        [HttpPost]
        [Route("categories")]
        public async Task<ActionResult<CategoryIndexedSearchResult>> SearchCategories([FromBody] CategoryIndexedSearchCriteria criteria)
        {
            criteria.ObjectType = KnownDocumentTypes.Category;
            var result = await _categoryIndexedSearchService.SearchAsync(criteria);
            //It is a important to return serialized data by such way. Instead you have a slow response time for large outputs 
            //https://github.com/dotnet/aspnetcore/issues/19646
            return Content(JsonConvert.SerializeObject(result, _jsonOptions.SerializerSettings), "application/json");
        }
    }
}
