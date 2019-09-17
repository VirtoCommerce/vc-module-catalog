using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/listentries")]
    public class CatalogModuleListEntryController : CatalogBaseController
    {
        private readonly ICatalogSearchService _searchService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;
        private readonly IItemService _itemService;
        private readonly IListEntrySearchService _listEntrySearchService;
        private readonly IListEntryMover<coreModel.Category> _categoryMover;
        private readonly IListEntryMover<coreModel.CatalogProduct> _productMover;

        public CatalogModuleListEntryController(
            ICatalogSearchService searchService,
            ICategoryService categoryService,
            IItemService itemService,
            ISecurityService securityService,
            IPermissionScopeService permissionScopeService,
            ICatalogService catalogService,
            IListEntrySearchService listEntrySearchService,
            IListEntryMover<coreModel.Category> categoryMover,
            IListEntryMover<coreModel.CatalogProduct> productMover
            )
            : base(securityService, permissionScopeService)
        {
            _searchService = searchService;
            _categoryService = categoryService;
            _itemService = itemService;
            _catalogService = catalogService;
            _listEntrySearchService = listEntrySearchService;
            _categoryMover = categoryMover;
            _productMover = productMover;
        }

        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(webModel.ListEntrySearchResult))]
        public IHttpActionResult ListItemsSearch(webModel.SearchCriteria criteria)
        {
            var coreModelCriteria = criteria.ToCoreModel();
            ApplyRestrictionsForCurrentUser(coreModelCriteria);

            coreModelCriteria.WithHidden = true;
            //Need search in children categories if user specify keyword
            if (!string.IsNullOrEmpty(coreModelCriteria.Keyword))
            {
                coreModelCriteria.SearchInChildren = true;
                coreModelCriteria.SearchInVariations = true;
            }

            var result = _listEntrySearchService.Search(coreModelCriteria);

            return Ok(result);
        }

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <param name="links">The links.</param>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks")]
        [ResponseType(typeof(void))]
        public IHttpActionResult CreateLinks(webModel.ListEntryLink[] links)
        {
            //Scope bound security check
            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, links);

            InnerUpdateLinks(links, (x, y) => x.Links.Add(y));
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Bulk create links to categories and items
        /// </summary>
        /// <param name="creationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks/bulkcreate")]
        [ResponseType(typeof(void))]
        public IHttpActionResult BulkCreateLinks(webModel.BulkLinkCreationRequest creationRequest)
        {
            if (creationRequest.CatalogId.IsNullOrEmpty())
            {
                throw new ArgumentException("Target catalog identifier should be specified.");
            }

            var coreModelCriteria = creationRequest.SearchCriteria.ToCoreModel();
            bool haveProducts;

            do
            {
                var links = new List<webModel.ListEntryLink>();

                var searchResult = _searchService.Search(coreModelCriteria);

                var productLinks = searchResult
                    .Products
                    .Select(x => new webModel.ListEntryLink
                    {
                        CatalogId = creationRequest.CatalogId,
                        ListEntryType = webModel.ListEntryProduct.TypeName,
                        ListEntryId = x.Id,
                        CategoryId = creationRequest.CategoryId
                    })
                    .ToList();

                links.AddRange(productLinks);

                if (coreModelCriteria.ResponseGroup.HasFlag(coreModel.SearchResponseGroup.WithCategories))
                {
                    coreModelCriteria.ResponseGroup = coreModelCriteria.ResponseGroup & ~coreModel.SearchResponseGroup.WithCategories;

                    var categoryLinks = searchResult
                        .Categories
                        .Select(c => new webModel.ListEntryLink
                        {
                            CatalogId = creationRequest.CatalogId,
                            ListEntryType = webModel.ListEntryCategory.TypeName,
                            ListEntryId = c.Id,
                            CategoryId = creationRequest.CategoryId
                        })
                        .ToList();

                    links.AddRange(categoryLinks);
                }

                haveProducts = productLinks.Any();

                coreModelCriteria.Skip += coreModelCriteria.Take;

                CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, links.ToArray());

                InnerUpdateLinks(links.ToArray(), (x, y) => x.Links.Add(y));

            } while (haveProducts);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("~/api/catalog/getslug")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetSlug(string text)
        {
            if (text == null)
            {
                return InternalServerError(new NullReferenceException("text"));
            }
            return Ok(text.GenerateSlug());
        }

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <param name="links">The links.</param>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks/delete")]
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteLinks(webModel.ListEntryLink[] links)
        {
            //Scope bound security check
            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Delete, links);

            InnerUpdateLinks(links, (x, y) => x.Links = x.Links.Where(l => string.Join(":", l.CatalogId, l.CategoryId) != string.Join(":", y.CatalogId, y.CategoryId)).ToList());
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <param name="moveInfo">Move operation details</param>
        [HttpPost]
        [Route("move")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Move(webModel.MoveInfo moveInfo)
        {
            var dstCatalog = _catalogService.GetById(moveInfo.Catalog);
            if (dstCatalog.IsVirtual)
            {
                throw new InvalidOperationException("Unable to move in virtual catalog");
            }

            var categories = _categoryMover.PrepareMove(moveInfo);
            var products = _productMover.PrepareMove(moveInfo);

            //Scope bound security check
            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, categories);
            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.Create, products);

            _categoryMover.ConfirmMove(categories);
            _productMover.ConfirmMove(products);

            return Ok();
        }

        private void InnerUpdateLinks(webModel.ListEntryLink[] links, Action<coreModel.ILinkSupport, coreModel.CategoryLink> action)
        {
            var changedObjects = new List<coreModel.ILinkSupport>();
            foreach (var link in links)
            {
                coreModel.ILinkSupport changedObject;
                var newlink = new coreModel.CategoryLink
                {
                    CategoryId = link.CategoryId,
                    CatalogId = link.CatalogId
                };

                if (link.ListEntryType.EqualsInvariant(webModel.ListEntryCategory.TypeName))
                {
                    changedObject = _categoryService.GetById(link.ListEntryId, coreModel.CategoryResponseGroup.Full);
                }
                else
                {
                    changedObject = _itemService.GetById(link.ListEntryId, coreModel.ItemResponseGroup.ItemLarge);
                }
                action(changedObject, newlink);
                changedObjects.Add(changedObject);
            }

            _categoryService.Update(changedObjects.OfType<coreModel.Category>().ToArray());
            _itemService.Update(changedObjects.OfType<coreModel.CatalogProduct>().ToArray());
        }
    }
}
