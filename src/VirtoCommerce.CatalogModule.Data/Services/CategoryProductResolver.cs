using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryProductResolver(Func<ICatalogRepository> repositoryFactory) : ICategoryProductResolver
    {
        public async Task<IList<string>> GetCategoryProductIds(string categoryId)
        {
            using var repository = repositoryFactory();
            var allCategoryIds = await GetAllCategoryIdsAsync(repository, categoryId);

            return await GetProductIdsAsync(repository, allCategoryIds);
        }

        private static async Task<List<string>> GetAllCategoryIdsAsync(ICatalogRepository repository, string rootCategoryId)
        {
            var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync([rootCategoryId]);
            var visited = new HashSet<string>(childCategoryIds, StringComparer.OrdinalIgnoreCase) { rootCategoryId };
            var categoriesToExpand = visited.ToList();

            while (categoriesToExpand.Count > 0)
            {
                var linkedPhysicalCategoryIds = await repository.CategoryLinks
                    .Where(x => categoriesToExpand.Contains(x.TargetCategoryId) && x.SourceCategoryId != null)
                    .Select(x => x.SourceCategoryId)
                    .Distinct()
                    .ToListAsync();

                var newLinkedCategoryIds = linkedPhysicalCategoryIds.Where(visited.Add).ToList();
                if (newLinkedCategoryIds.Count == 0)
                {
                    break;
                }

                var newChildCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(newLinkedCategoryIds);
                foreach (var id in newChildCategoryIds)
                {
                    visited.Add(id);
                }

                categoriesToExpand = newLinkedCategoryIds;
            }

            return visited.ToList();
        }

        private static async Task<List<string>> GetProductIdsAsync(ICatalogRepository repository, List<string> categoryIds)
        {
            var mainProducts = repository.Items.Where(i => i.ParentId == null);

            var directIds = await mainProducts
                .Where(i => categoryIds.Contains(i.CategoryId))
                .Select(i => i.Id)
                .ToListAsync();

            var linkedIds = await repository.CategoryItemRelations
                .Where(r => categoryIds.Contains(r.CategoryId) && mainProducts.Any(i => i.Id == r.ItemId))
                .Select(r => r.ItemId)
                .Distinct()
                .ToListAsync();

            return directIds.Union(linkedIds, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
