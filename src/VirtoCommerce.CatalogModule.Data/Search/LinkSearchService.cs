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

        public virtual async Task<LinkSearchResult> SearchAsync(LinkSearchCriteria criteria)
        {
            var result = new LinkSearchResult();

            using (var repository = _catalogRepositoryFactory())
            {
                repository.DisableChangesTracking();

                if (criteria.ObjectType.EqualsIgnoreCase(nameof(CatalogProduct)))
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

                        result.Results = productLinks.Select(ToCategoryLink).ToList();
                    }
                }
                else if (criteria.ObjectType.EqualsIgnoreCase(nameof(Category)))
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

                        result.Results = categoryLinks.Select(ToCategoryLink).ToList();
                    }
                }
            }

            foreach (var link in result.Results)
            {
                if (link.CategoryId != null)
                {
                    var category = await _categoryService.GetByIdsAsync([link.CategoryId], nameof(CategoryResponseGroup.WithOutlines), link.CatalogId);
                    link.Category = category.FirstOrDefault();
                }
                else
                {
                    link.Catalog = await _catalogService.GetByIdAsync(link.CatalogId, nameof(CatalogResponseGroup.Info));
                }
            }

            return result;
        }

        protected virtual CategoryLink ToCategoryLink(CategoryRelationEntity relation)
        {
            var link = AbstractTypeFactory<CategoryLink>.TryCreateInstance();

            link.ListEntryId = relation.SourceCategoryId;
            link.ListEntryType = nameof(Category);
            link.CatalogId = relation.TargetCatalogId;
            link.CategoryId = relation.TargetCategoryId;

            return link;
        }

        protected virtual CategoryLink ToCategoryLink(CategoryItemRelationEntity relation)
        {
            var link = AbstractTypeFactory<CategoryLink>.TryCreateInstance();

            link.ListEntryId = relation.ItemId;
            link.ListEntryType = nameof(CatalogProduct);
            link.CatalogId = relation.CatalogId;
            link.CategoryId = relation.CategoryId;
            link.IsAutomatic = relation.IsAutomatic;

            return link;
        }

        protected virtual IQueryable<CategoryItemRelationEntity> GetProductLinksQuery(IQueryable<CategoryItemRelationEntity> categoryItemRelations, LinkSearchCriteria criteria)
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

            if (criteria.IsAutomatic != null)
            {
                query = query.Where(x => x.IsAutomatic == criteria.IsAutomatic);
            }

            return query;
        }

        protected virtual IQueryable<CategoryRelationEntity> GetCategoryLinksQuery(IQueryable<CategoryRelationEntity> categoryLinks, LinkSearchCriteria criteria)
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
