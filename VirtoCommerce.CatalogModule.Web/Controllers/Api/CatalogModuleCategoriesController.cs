using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/categories")]
    public class CatalogModuleCategoriesController : CatalogBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly ICatalogSearchService _catalogSearchService;
        private const int DeleteBatchSize = 50;

        public CatalogModuleCategoriesController(ICategoryService categoryService, ICatalogService catalogService, IBlobUrlResolver blobUrlResolver, ISecurityService securityService, IPermissionScopeService permissionScopeService, ICatalogSearchService catalogSearchService)
            : base(securityService, permissionScopeService)
        {
            _categoryService = categoryService;
            _catalogService = catalogService;
            _blobUrlResolver = blobUrlResolver;
            _catalogSearchService = catalogSearchService;
        }


        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <param name="id">Category id.</param>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(webModel.Category))]
        public IHttpActionResult Get(string id)
        {
            var category = ((OkNegotiatedContentResult<webModel.Category[]>)GetCategoriesByIds(new[] { id })).Content.FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <param name="ids">Categories ids</param>
        ///<param name="respGroup">Response group.</param>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(webModel.Category[]))]
        public IHttpActionResult GetCategoriesByIds([FromUri] string[] ids, [FromUri] coreModel.CategoryResponseGroup respGroup = coreModel.CategoryResponseGroup.Full)
        {
            var categories = _categoryService.GetByIds(ids, respGroup);

            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Read, categories);

            var retVal = categories.Select(x => x.ToWebModel(_blobUrlResolver)).ToArray();
            foreach (var category in retVal)
            {
                category.SecurityScopes = GetObjectPermissionScopeStrings(category);
            }

            return Ok(retVal);
        }

        /// <summary>
        /// Get categories by plenty ids
        /// </summary>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group</param>
        /// <returns></returns>
        [HttpPost]
        [Route("plenty")]
        [ResponseType(typeof(webModel.Category[]))]
        public IHttpActionResult GetCategoriesByPlentyIds([FromBody] string[] ids, [FromUri] coreModel.CategoryResponseGroup respGroup = coreModel.CategoryResponseGroup.Full)
        {
            return GetCategoriesByIds(ids, respGroup);
        }

        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional)</param>
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/categories/newcategory")]
        [ResponseType(typeof(webModel.Category))]
        public IHttpActionResult GetNewCategory(string catalogId, [FromUri]string parentCategoryId = null)
        {
            var retVal = new webModel.Category
            {
                ParentId = parentCategoryId,
                CatalogId = catalogId,
                Code = Guid.NewGuid().ToString().Substring(0, 5),
                SeoInfos = new List<SeoInfo>(),
                IsActive = true
            };

            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, retVal.ToModuleModel());
            retVal.SecurityScopes = GetObjectPermissionScopeStrings(retVal.ToModuleModel());

            return Ok(retVal);
        }


        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>If category.id is null, a new category is created. It's updated otherwise</remarks>
        /// <param name="category">The category.</param>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CreateOrUpdateCategory(webModel.Category category)
        {
            var coreCategory = category.ToModuleModel();
            if (coreCategory.Id == null)
            {
                if (coreCategory.SeoInfos == null || !coreCategory.SeoInfos.Any())
                {
                    var slugUrl = category.Name.GenerateSlug();
                    if (!String.IsNullOrEmpty(slugUrl))
                    {
                        var catalog = _catalogService.GetById(category.CatalogId);
                        var defaultLanguage = catalog.Languages.First(x => x.IsDefault).LanguageCode;
                        coreCategory.SeoInfos = new[] { new SeoInfo { LanguageCode = defaultLanguage, SemanticUrl = slugUrl } };
                    }
                }

                CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, coreCategory);

                var retVal = _categoryService.Create(coreCategory).ToWebModel(_blobUrlResolver);
                return Ok(retVal);
            }
            else
            {
                CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Update, coreCategory);

                _categoryService.Update(new[] { coreCategory });
                return StatusCode(HttpStatusCode.NoContent);
            }
        }


        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <param name="ids">The categories ids.</param>
        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete([FromUri]string[] ids)
        {
            var categories = _categoryService.GetByIds(ids, Domain.Catalog.Model.CategoryResponseGroup.WithParents);
            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Delete, categories);

            _categoryService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Bulk deletes the specified categories by id.
        /// </summary>
        /// <param name="categorySearchCriteria"></param>
        [HttpPost]
        [Route("delete")]
        [ResponseType(typeof(void))]
        public IHttpActionResult BulkDelete(CategorySearchCriteria categorySearchCriteria)
        {
            var idsToDelete = categorySearchCriteria.ObjectIds?.ToList() ?? new List<string>();

            if (idsToDelete.IsNullOrEmpty())
            {
                idsToDelete = GetIdsToDelete(categorySearchCriteria);
            }
            else
            {
                idsToDelete.ProcessWithPaging(DeleteBatchSize, (ids, currentItem, totalCount) =>
                {
                    var searchResult = _categoryService.GetByIds(ids.ToArray(), coreModel.CategoryResponseGroup.Info);
                    CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Delete, searchResult);
                });
            }

            idsToDelete.ProcessWithPaging(DeleteBatchSize, (ids, currentItem, totalCount) =>
            {
                _categoryService.Delete(ids.ToArray());
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

        private List<string> GetIdsToDelete(CategorySearchCriteria categorySearchCriteria)
        {
            var searchCriteria = categorySearchCriteria.ToSearchCriteria();
            // Any pagination for deleting should be managed at back-end. 
            searchCriteria.Take = DeleteBatchSize;
            searchCriteria.Skip = 0;

            var result = new List<string>();
            bool hasItems;
            do
            {
                var searchResult = _catalogSearchService.Search(searchCriteria);

                hasItems = searchResult.Categories.Any();
                if (hasItems)
                {
                    CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Delete, searchResult.Categories);
                    result.AddRange(searchResult.Categories.Select(x => x.Id));
                    searchCriteria.Skip += searchCriteria.Take;
                }
            }
            while (hasItems);

            return result;
        }
    }
}
