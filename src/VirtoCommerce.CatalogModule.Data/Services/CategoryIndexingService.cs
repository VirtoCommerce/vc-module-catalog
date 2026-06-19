using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryIndexingService(Func<ICatalogRepository> repositoryFactory) : ICategoryIndexingService
    {
        public async Task<IList<string>> GetProductIdsForIndexAsync(string categoryId)
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
                var virtualCategoryTargetIds = await repository.CategoryLinks
                    .Where(x => categoriesToExpand.Contains(x.SourceCategoryId) && x.TargetCategoryId != null)
                    .Select(x => x.TargetCategoryId)
                    .Distinct()
                    .ToListAsync();

                var newCategoryTargetIds = virtualCategoryTargetIds.Where(visited.Add).ToList();
                if (newCategoryTargetIds.Count == 0)
                {
                    break;
                }

                var newChildCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(newCategoryTargetIds);
                foreach (var id in newChildCategoryIds)
                {
                    visited.Add(id);
                }

                categoriesToExpand = newCategoryTargetIds;
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
