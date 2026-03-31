using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/listentries")]
    [Authorize]
    public class CatalogModuleListEntryController(
        IInternalListEntrySearchService internalListEntrySearchService,
        ILinkSearchService linkSearchService,
        ICategoryService categoryService,
        IItemService itemService,
        ICatalogService catalogService,
        CatalogEntityAuthorizationService catalogEntityAuthorizationService,
        ListEntryMover<Category> categoryMover,
        ListEntryMover<CatalogProduct> productMover
        ) : Controller
    {
        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<ListEntrySearchResult>> ListItemsSearchAsync([FromBody] CatalogListEntrySearchCriteria criteria)
        {
            var authorizedCriteria = await catalogEntityAuthorizationService.GetAuthorizedCriteriaByTypeAsync(
                User,
                criteria,
                ModuleConstants.Security.Permissions.CategoriesRead,
                ModuleConstants.Security.Permissions.ProductsRead,
                ModuleConstants.Security.Permissions.Read);
            if (authorizedCriteria.Count == 0)
            {
                return Forbid();
            }

            var result = await SearchAuthorizedListEntriesAsync(authorizedCriteria);

            return Ok(result);
        }


        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <param name="links">The links.</param>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks")]
        public async Task<ActionResult> CreateLinks([FromBody] CategoryLink[] links)
        {
            var entryIds = links.Select(x => x.EntryId).ToArray();
            var hasLinkEntries = await LoadCatalogEntriesAsync<IHasLinks>(entryIds);

            if (!await catalogEntityAuthorizationService.TryAuthorizeEntitiesByTypeAsync(
                User,
                hasLinkEntries,
                ModuleConstants.Security.Permissions.CategoriesUpdate,
                ModuleConstants.Security.Permissions.ProductsUpdate,
                ModuleConstants.Security.Permissions.Update))
            {
                return Forbid();
            }

            foreach (var link in links)
            {
                var hasLinkEntry = hasLinkEntries.FirstOrDefault(x => x.Id.Equals(link.EntryId));
                if (hasLinkEntry != null && !hasLinkEntry.Links.Contains(link))
                {
                    hasLinkEntry.Links.Add(link);
                }
            }

            if (!hasLinkEntries.IsNullOrEmpty())
            {
                await SaveListCatalogEntitiesAsync(hasLinkEntries.ToArray());
            }

            return Ok();
        }


        /// <summary>
        /// Creates category links in bulk for catalog entries based on the specified search criteria.
        /// </summary>
        /// <param name="creationRequest">The request containing the target catalog, category, and search criteria for the entries to link.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="ActionResult"/> indicating the result of the operation. Returns <see cref="BadRequestResult"/>
        /// if the catalog identifier is not specified, <see cref="ForbidResult"/> if the user lacks the required
        /// permissions, or <see cref="OkResult"/> upon successful completion.</returns>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks/bulkcreate")]
        public async Task<ActionResult> BulkCreateLinks([FromBody] BulkLinkCreationRequest creationRequest, CancellationToken cancellationToken)
        {
            if (creationRequest.CatalogId.IsNullOrEmpty())
            {
                return BadRequest("Target catalog identifier should be specified.");
            }

            var authorizedCriteria = await catalogEntityAuthorizationService.GetAuthorizedCriteriaByTypeAsync(
                User,
                creationRequest.SearchCriteria,
                ModuleConstants.Security.Permissions.CategoriesUpdate,
                ModuleConstants.Security.Permissions.ProductsUpdate,
                ModuleConstants.Security.Permissions.Update);
            if (authorizedCriteria.Count == 0)
            {
                return Forbid();
            }

            var searchResult = await SearchAllAuthorizedListEntriesAsync(authorizedCriteria, cancellationToken);
            var hasLinkEntries = await LoadCatalogEntriesAsync<IHasLinks>(searchResult.Select(x => x.Id).ToArray());

            if (hasLinkEntries.Count == 0)
            {
                return Ok();
            }

            if (!await catalogEntityAuthorizationService.TryAuthorizeEntitiesByTypeAsync(
                User,
                hasLinkEntries,
                ModuleConstants.Security.Permissions.CategoriesUpdate,
                ModuleConstants.Security.Permissions.ProductsUpdate,
                ModuleConstants.Security.Permissions.Update))
            {
                return Forbid();
            }

            foreach (var hasLinkEntry in hasLinkEntries)
            {
                var link = AbstractTypeFactory<CategoryLink>.TryCreateInstance();
                link.CategoryId = creationRequest.CategoryId;
                link.CatalogId = creationRequest.CatalogId;
                hasLinkEntry.Links.Add(link);
            }

            await SaveListCatalogEntitiesAsync(hasLinkEntries.ToArray());

            return Ok();
        }

        [HttpPost]
        [Route("~/api/catalog/listentrylinks/search")]
        public async Task<ActionResult> SearchLinks([FromBody] LinkSearchCriteria criteria)
        {
            var objectIds = criteria.ObjectIds ?? Enumerable.Empty<string>();
            var categoryIds = criteria.CategoryIds ?? Enumerable.Empty<string>();
            var entryIds = objectIds.Concat(categoryIds).Distinct().ToArray();

            var hasLinkEntries = await LoadCatalogEntriesAsync<IHasLinks>(entryIds);

            if (!await catalogEntityAuthorizationService.TryAuthorizeEntitiesByTypeAsync(
                User,
                hasLinkEntries,
                ModuleConstants.Security.Permissions.CategoriesRead,
                ModuleConstants.Security.Permissions.ProductsRead,
                ModuleConstants.Security.Permissions.Read))
            {
                return Forbid();
            }

            var result = await linkSearchService.SearchAsync(criteria);

            return Ok(result);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("~/api/catalog/getslug")]
        public ActionResult<string> GetSlug(string text)
        {
            return Ok(text.GenerateSlug());
        }

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <param name="links">The links.</param>
        [HttpPost]
        [Route("~/api/catalog/listentrylinks/delete")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteLinks([FromBody] CategoryLink[] links)
        {
            var entryIds = links.Select(x => x.EntryId).ToArray();
            var hasLinkEntries = await LoadCatalogEntriesAsync<IHasLinks>(entryIds);

            if (!await catalogEntityAuthorizationService.TryAuthorizeEntitiesByTypeAsync(
                User,
                hasLinkEntries,
                ModuleConstants.Security.Permissions.CategoriesDelete,
                ModuleConstants.Security.Permissions.ProductsDelete,
                ModuleConstants.Security.Permissions.Delete))
            {
                return Forbid();
            }

            foreach (var link in links)
            {
                var hasLinkEntry = hasLinkEntries.FirstOrDefault(x => x.Id.Equals(link.EntryId));
                hasLinkEntry?.Links.Remove(link);
            }

            if (!hasLinkEntries.IsNullOrEmpty())
            {
                await SaveListCatalogEntitiesAsync(hasLinkEntries.ToArray());
            }

            return NoContent();
        }

        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <param name="moveRequest">Move operation request</param>
        [HttpPost]
        [Route("move")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Move([FromBody] ListEntriesMoveRequest moveRequest)
        {
            var sourceEntries = await LoadCatalogEntriesAsync<IEntity>(moveRequest.ListEntries?.Select(x => x.Id).ToArray() ?? []);
            if (!await catalogEntityAuthorizationService.TryAuthorizeMoveRequestByTypeAsync(
                User,
                moveRequest,
                sourceEntries,
                ModuleConstants.Security.Permissions.CategoriesUpdate,
                ModuleConstants.Security.Permissions.ProductsUpdate,
                ModuleConstants.Security.Permissions.Update))
            {
                return Forbid();
            }

            var dstCatalog = await catalogService.GetNoCloneAsync(moveRequest.Catalog);
            if (dstCatalog.IsVirtual)
            {
                return BadRequest("Unable to move to a virtual catalog");
            }

            var categories = await categoryMover.PrepareMoveAsync(moveRequest);
            var products = await productMover.PrepareMoveAsync(moveRequest);

            await categoryMover.ConfirmMoveAsync(categories);
            await productMover.ConfirmMoveAsync(products);

            return NoContent();
        }

        /// <summary>
        /// Bulk delete by the search criteria.
        /// </summary>
        /// <param name="criteria"></param>
        [HttpPost]
        [Route("delete")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Delete([FromBody] CatalogListEntrySearchCriteria criteria)
        {
            const int deleteBatchSize = 20;

            var idsToDelete = criteria.ObjectIds?.ToList() ?? new List<string>();

            if (!idsToDelete.IsNullOrEmpty())
            {
                var entitiesToDelete = await LoadCatalogEntriesAsync<IEntity>(idsToDelete.ToArray());
                if (!await catalogEntityAuthorizationService.TryAuthorizeEntitiesByTypeAsync(
                    User,
                    entitiesToDelete,
                    ModuleConstants.Security.Permissions.CategoriesDelete,
                    ModuleConstants.Security.Permissions.ProductsDelete,
                    ModuleConstants.Security.Permissions.Delete))
                {
                    return Forbid();
                }
            }
            else
            {
                var authorizedCriteria = await catalogEntityAuthorizationService.GetAuthorizedCriteriaByTypeAsync(
                    User,
                    criteria,
                    ModuleConstants.Security.Permissions.CategoriesDelete,
                    ModuleConstants.Security.Permissions.ProductsDelete,
                    ModuleConstants.Security.Permissions.Delete);
                if (authorizedCriteria.Count == 0)
                {
                    return Forbid();
                }

                var listEntries = await SearchAllAuthorizedListEntriesAsync(authorizedCriteria, CancellationToken.None);
                idsToDelete = listEntries
                    .Where(x => x.Type.EqualsIgnoreCase(ProductListEntry.TypeName) || x.Type.EqualsIgnoreCase(CategoryListEntry.TypeName))
                    .Select(x => x.Id)
                    .ToList();
            }

            for (var i = 0; i < idsToDelete.Count; i += deleteBatchSize)
            {
                var commonIds = idsToDelete.Skip(i).Take(deleteBatchSize).ToList();

                var searchProductResult = await itemService.GetNoCloneAsync(commonIds, ItemResponseGroup.None.ToString());
                await itemService.DeleteAsync(searchProductResult.Select(x => x.Id).ToArray());

                var searchCategoryResult = await categoryService.GetNoCloneAsync(commonIds, CategoryResponseGroup.None.ToString());
                await categoryService.DeleteAsync(searchCategoryResult.Select(x => x.Id).ToArray());
            }

            return NoContent();
        }

        private async Task<ListEntrySearchResult> SearchAuthorizedListEntriesAsync(IList<CatalogListEntrySearchCriteria> authorizedCriteria)
        {
            if (authorizedCriteria.Count == 1)
            {
                return await internalListEntrySearchService.InnerListEntrySearchAsync(authorizedCriteria[0]);
            }

            var result = new ListEntrySearchResult();
            var categoryCriteria = authorizedCriteria.FirstOrDefault(x => x.ObjectTypes?.Contains(nameof(Category)) ?? false);
            var productCriteria = authorizedCriteria.FirstOrDefault(x => x.ObjectTypes?.Contains(nameof(CatalogProduct)) ?? false);

            var categorySkip = 0;
            var categoryTake = 0;

            if (categoryCriteria != null)
            {
                var categoryResult = await internalListEntrySearchService.InnerListEntrySearchAsync(categoryCriteria);
                categorySkip = Math.Min(categoryResult.TotalCount, categoryCriteria.Skip);
                categoryTake = Math.Min(categoryCriteria.Take, Math.Max(0, categoryResult.TotalCount - categoryCriteria.Skip));

                result.TotalCount = categoryResult.TotalCount;
                result.Results.AddRange(categoryResult.Results);
            }

            if (productCriteria != null)
            {
                var pagedProductCriteria = productCriteria.CloneTyped();
                pagedProductCriteria.Skip = Math.Max(0, pagedProductCriteria.Skip - categorySkip);
                pagedProductCriteria.Take = Math.Max(0, pagedProductCriteria.Take - categoryTake);

                var productResult = await internalListEntrySearchService.InnerListEntrySearchAsync(pagedProductCriteria);
                result.TotalCount += productResult.TotalCount;
                result.Results.AddRange(productResult.Results);
            }

            return result;
        }

        private async Task<IList<ListEntryBase>> SearchAllAuthorizedListEntriesAsync(IList<CatalogListEntrySearchCriteria> authorizedCriteria, CancellationToken cancellationToken)
        {
            var result = new List<ListEntryBase>();
            var cancellationTokenWrapper = new CancellationTokenWrapper(cancellationToken);

            foreach (var authorizedSearchCriteria in authorizedCriteria)
            {
                var searchCriteria = authorizedSearchCriteria.CloneTyped();
                searchCriteria.Skip = 0;

                result.AddRange(await internalListEntrySearchService.SearchAllAsync(searchCriteria, cancellationTokenWrapper));
            }

            return result;
        }

        private async Task SaveListCatalogEntitiesAsync(IEntity[] entities)
        {
            if (!entities.IsNullOrEmpty())
            {
                var products = entities.OfType<CatalogProduct>().ToArray();
                if (!products.IsNullOrEmpty())
                {
                    await itemService.SaveChangesAsync(products);
                }

                var categories = entities.OfType<Category>().ToArray();
                if (!categories.IsNullOrEmpty())
                {
                    await categoryService.SaveChangesAsync(categories);
                }
            }
        }

        private async Task<IList<T>> LoadCatalogEntriesAsync<T>(string[] ids)
        {
#pragma warning disable CS0618 // Variations can be used here
            var products = await itemService.GetAsync(ids, (ItemResponseGroup.Links | ItemResponseGroup.ItemProperties | ItemResponseGroup.Variations).ToString());
#pragma warning restore CS0618
            var categories = await categoryService.GetAsync(ids.Except(products.Select(x => x.Id)).ToList(), (CategoryResponseGroup.WithLinks).ToString());
            return products.OfType<T>().Concat(categories.OfType<T>()).ToList();
        }
    }
}
