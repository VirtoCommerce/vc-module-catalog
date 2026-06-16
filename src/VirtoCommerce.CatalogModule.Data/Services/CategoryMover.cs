using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryMover : ListEntryMover<Category>
    {
        private const int CategoryPageSize = 200;
        private const int CategoryIdsChunkSize = 10;
        private const int ProductPageSize = 50;

        private readonly ICategoryService _categoryService;
        private readonly ICategorySearchService _categorySearchService;
        private readonly IProductSearchService _productSearchService;
        private readonly IItemService _itemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryMover"/> class.
        /// </summary>
        public CategoryMover(
            ICategoryService categoryService,
            ICategorySearchService categorySearchService,
            IProductSearchService productSearchService,
            IItemService itemService)
        {
            _categoryService = categoryService;
            _categorySearchService = categorySearchService;
            _productSearchService = productSearchService;
            _itemService = itemService;
        }

        public override async Task ConfirmMoveAsync(IEnumerable<Category> entities)
        {
            var categories = entities as IList<Category> ?? entities.ToList();
            if (categories.Count == 0)
            {
                return;
            }

            await _categoryService.SaveChangesAsync(categories.ToArray());

            // Cascade CatalogId to products that live under any of the moved categories.
            // Every entity here carries the target CatalogId (set in PrepareMoveAsync), and the
            // per-product filter in CascadeProductsAsync makes same-catalog reparents a cheap no-op.
            var targetCatalogId = categories[0].CatalogId;
            var categoryIds = categories.Select(x => x.Id).ToArray();

            await CascadeProductsAsync(categoryIds, targetCatalogId);
        }

        public override async Task<List<Category>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            await ValidateOperationArguments(moveInfo);

            var result = new List<Category>();

            // Cross-catalog roots grouped by their original (source) CatalogId so we can load
            // descendants per source catalog with a single paged search each.
            var crossCatalogRoots = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var listEntryCategory in moveInfo.ListEntries.Where(
                listEntry => listEntry.Type.EqualsIgnoreCase(CategoryListEntry.TypeName)))
            {
                var category = await _categoryService.GetByIdAsync(listEntryCategory.Id, CategoryResponseGroup.Info.ToString());

                if (category.CatalogId != moveInfo.Catalog)
                {
                    if (!crossCatalogRoots.TryGetValue(category.CatalogId, out var roots))
                    {
                        roots = new List<string>();
                        crossCatalogRoots.Add(category.CatalogId, roots);
                    }
                    roots.Add(category.Id);

                    category.CatalogId = moveInfo.Catalog;
                }

                if (category.ParentId != moveInfo.Category)
                {
                    category.ParentId = moveInfo.Category;
                }

                result.Add(category);
            }

            if (crossCatalogRoots.Count > 0)
            {
                var descendants = await LoadDescendantsAsync(crossCatalogRoots);
                foreach (var descendant in descendants)
                {
                    descendant.CatalogId = moveInfo.Catalog;
                    result.Add(descendant);
                }
            }

            return result;
        }

        protected virtual async Task ValidateOperationArguments(ListEntriesMoveRequest moveInfo)
        {
            if (moveInfo == null)
            {
                throw new ArgumentNullException(nameof(moveInfo));
            }

            var validator = new ListEntriesMoveRequestValidator(_categoryService);
            await validator.ValidateAndThrowAsync(moveInfo);
        }

        private async Task<List<Category>> LoadDescendantsAsync(Dictionary<string, List<string>> rootsBySourceCatalog)
        {
            var all = new List<Category>();

            foreach (var pair in rootsBySourceCatalog)
            {
                var byParent = await BuildParentChildrenMapAsync(pair.Key);
                CollectDescendants(byParent, pair.Value, all);
            }

            return all;
        }

        // Page through the entire source catalog and build a parent -> children map in memory.
        // ICategorySearchService doesn't accept multiple parent ids, so a single catalog-wide
        // load is more efficient than one search per node when the moved subtree is non-trivial.
        private async Task<Dictionary<string, List<Category>>> BuildParentChildrenMapAsync(string sourceCatalogId)
        {
            var byParent = new Dictionary<string, List<Category>>(StringComparer.OrdinalIgnoreCase);
            var skip = 0;

            while (true)
            {
                var page = await SearchCategoryPageAsync(sourceCatalogId, skip);
                if (page.Count == 0)
                {
                    break;
                }

                foreach (var category in page)
                {
                    AddToParentMap(byParent, category);
                }

                if (page.Count < CategoryPageSize)
                {
                    break;
                }
                skip += CategoryPageSize;
            }

            return byParent;
        }

        private async Task<IList<Category>> SearchCategoryPageAsync(string sourceCatalogId, int skip)
        {
            var criteria = AbstractTypeFactory<CategorySearchCriteria>.TryCreateInstance();
            criteria.CatalogId = sourceCatalogId;
            criteria.ResponseGroup = CategoryResponseGroup.Info.ToString();
            criteria.Skip = skip;
            criteria.Take = CategoryPageSize;

            var result = await _categorySearchService.SearchAsync(criteria);
            return result.Results;
        }

        private static void AddToParentMap(Dictionary<string, List<Category>> byParent, Category category)
        {
            var parentKey = category.ParentId ?? string.Empty;
            if (!byParent.TryGetValue(parentKey, out var children))
            {
                byParent[parentKey] = children = new List<Category>();
            }

            children.Add(category);
        }

        // BFS from each moved root. Roots themselves are seeded as visited so we don't re-add
        // them (they're already in the result list of PrepareMoveAsync).
        private static void CollectDescendants(Dictionary<string, List<Category>> byParent, IList<string> rootIds, List<Category> output)
        {
            var visited = new HashSet<string>(rootIds, StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(rootIds);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                if (!byParent.TryGetValue(parentId, out var children))
                {
                    continue;
                }

                // visited.Add returns true for ids not seen yet; the side-effect runs as Where enumerates.
                foreach (var child in children.Where(x => visited.Add(x.Id)))
                {
                    output.Add(child);
                    queue.Enqueue(child.Id);
                }
            }
        }

        private async Task CascadeProductsAsync(IList<string> categoryIds, string targetCatalogId)
        {
            foreach (var idsChunk in categoryIds.Chunk(CategoryIdsChunkSize))
            {
                var skip = 0;
                while (true)
                {
                    var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
                    criteria.CategoryIds = idsChunk;
                    criteria.SearchInVariations = true;
                    criteria.ResponseGroup = ItemResponseGroup.ItemLarge.ToString();
                    criteria.Skip = skip;
                    criteria.Take = ProductPageSize;

                    var page = await _productSearchService.SearchAsync(criteria);
                    if (page.Results.Count == 0)
                    {
                        break;
                    }

                    var toSave = page.Results
                        .Where(x => x.CatalogId != targetCatalogId)
                        .ToList();

                    foreach (var product in toSave)
                    {
                        product.CatalogId = targetCatalogId;
                    }

                    if (toSave.Count > 0)
                    {
                        await _itemService.SaveChangesAsync(toSave.ToArray());
                    }

                    if (page.Results.Count < ProductPageSize)
                    {
                        break;
                    }

                    skip += ProductPageSize;
                }
            }
        }
    }
}
