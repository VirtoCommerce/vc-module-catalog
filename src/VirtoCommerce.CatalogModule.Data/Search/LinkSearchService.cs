using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class LinkSearchService : ILinkSearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;

        public LinkSearchService(Func<ICatalogRepository> catalogRepositoryFactory, ICategoryService categoryService, ICatalogService catalogService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _categoryService = categoryService;
            _catalogService = catalogService;
        }

        public async Task<LinkSearchResult> SearchAsync(LinkSearchCriteria criteria)
        {
            var result = new LinkSearchResult();

            using (var repository = _catalogRepositoryFactory())
            {
                repository.DisableChangesTracking();

                if (criteria.ObjectType.EqualsInvariant(nameof(CatalogProduct)))
                {
                    var productLinksQuery = GetProductLinksQuery(repository.CategoryItemRelations, criteria);

                    result.TotalCount = await productLinksQuery.CountAsync();

                    if (result.TotalCount > 0 && criteria.Take > 0)
                    {
                        var productLinks = await productLinksQuery
                            .OrderBy(x => x.ItemId)
                            .Skip(criteria.Skip).Take(criteria.Take)
                            .AsNoTracking()
                            .ToListAsync();

                        result.Results = productLinks.Select(x => ToCategoryLink(x)).ToList();
                    }
                }
                else if (criteria.ObjectType.EqualsInvariant(nameof(Category)))
                {
                    var categoryLinksQuery = GetCategoryLinksQuery(repository.CategoryLinks, criteria);

                    result.TotalCount = await categoryLinksQuery.CountAsync();

                    if (result.TotalCount > 0 && criteria.Take > 0)
                    {
                        var categoryLinks = await categoryLinksQuery
                            .OrderBy(x => x.SourceCategoryId)
                            .Skip(criteria.Skip).Take(criteria.Take)
                            .AsNoTracking()
                            .ToListAsync();

                        result.Results = categoryLinks.Select(x => ToCategoryLink(x)).ToList();
                    }
                }
            }

            foreach (var link in result.Results)
            {
                if (link.CategoryId != null)
                {
                    var category = await _categoryService.GetByIdsAsync(new string[] { link.CategoryId }, CategoryResponseGroup.WithOutlines.ToString(), link.CatalogId);
                    link.Category = category.FirstOrDefault();
                }
                else
                {
                    var catalog = await _catalogService.GetByIdsAsync(new string[] { link.CatalogId }, CatalogResponseGroup.Info.ToString());
                    link.Catalog = catalog.FirstOrDefault();
                }
            }

            return result;
        }

        protected CategoryLink ToCategoryLink(CategoryRelationEntity productRelation)
        {
            var enitry = AbstractTypeFactory<CategoryLink>.TryCreateInstance();

            enitry.ListEntryId = productRelation.SourceCategoryId;
            enitry.ListEntryType = nameof(Category);
            enitry.CatalogId = productRelation.TargetCatalogId;
            enitry.CategoryId = productRelation.TargetCategoryId;

            return enitry;
        }

        protected CategoryLink ToCategoryLink(CategoryItemRelationEntity categoryRelation)
        {
            var enitry = AbstractTypeFactory<CategoryLink>.TryCreateInstance();

            enitry.ListEntryId = categoryRelation.ItemId;
            enitry.ListEntryType = nameof(CatalogProduct);
            enitry.CatalogId = categoryRelation.CatalogId;
            enitry.CategoryId = categoryRelation.CategoryId;

            return enitry;
        }

        protected IQueryable<CategoryItemRelationEntity> GetProductLinksQuery(IQueryable<CategoryItemRelationEntity> categoryItemRelations, LinkSearchCriteria criteria)
        {
            var query = categoryItemRelations;

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.ItemId));
            }

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CategoryIds.Contains(x.CategoryId));
            }
            else if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId) && x.CategoryId == null);
            }

            return query;
        }

        protected IQueryable<CategoryRelationEntity> GetCategoryLinksQuery(IQueryable<CategoryRelationEntity> categoryLinks, LinkSearchCriteria criteria)
        {
            var query = categoryLinks;

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.SourceCategoryId));
            }

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CategoryIds.Contains(x.TargetCategoryId));
            }
            else if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.TargetCatalogId) && x.TargetCategoryId == null);
            }

            return query;
        }
    }
}
