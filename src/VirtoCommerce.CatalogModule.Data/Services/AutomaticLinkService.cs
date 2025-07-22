using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class AutomaticLinkService(
    IAutomaticLinkQuerySearchService querySearchService,
    ICategoryService categoryService,
    IItemService itemService,
    IProductIndexedSearchService productIndexedSearchService,
    ILinkSearchService linkSearchService)
    : IAutomaticLinkService
{
    protected virtual int BatchSize { get; set; } = 50;

    public virtual async Task UpdateLinks(string categoryId, ICancellationToken cancellationToken)
    {
        var query = await GetQuery(categoryId);
        if (query is null)
        {
            return;
        }

        var category = await categoryService.GetByIdAsync(query.TargetCategoryId);
        if (category is null)
        {
            return;
        }

        var linksToRemove = await GetExistingLinks(categoryId, cancellationToken);

        if (!query.SourceCatalogId.IsNullOrWhiteSpace() && !query.SourceCatalogQuery.IsNullOrWhiteSpace())
        {
            await CreateLinks(category, query, linksToRemove, cancellationToken);
        }

        await DeleteAutomaticLinks(linksToRemove, cancellationToken);
    }

    public async Task DeleteLinks(string categoryId, ICancellationToken cancellationToken)
    {
        var linksToRemove = await GetExistingLinks(categoryId, cancellationToken);
        await DeleteAutomaticLinks(linksToRemove, cancellationToken);
    }


    protected virtual async Task<AutomaticLinkQuery> GetQuery(string categoryId)
    {
        var criteria = AbstractTypeFactory<AutomaticLinkQuerySearchCriteria>.TryCreateInstance();
        criteria.TargetCategoryId = categoryId;
        criteria.Take = 1;

        var searchResult = await querySearchService.SearchAsync(criteria);

        return searchResult.Results.FirstOrDefault();
    }

    protected virtual Task<IList<CategoryLink>> GetExistingLinks(string categoryId, ICancellationToken cancellationToken)
    {
        var criteria = AbstractTypeFactory<LinkSearchCriteria>.TryCreateInstance();
        criteria.CategoryIds = [categoryId];
        criteria.ObjectType = nameof(CatalogProduct);
        criteria.Take = BatchSize;

        return linkSearchService.SearchAllAsync(criteria, cancellationToken);
    }

    protected virtual async Task CreateLinks(Category category, AutomaticLinkQuery query, IList<CategoryLink> linksToRemove, ICancellationToken cancellationToken)
    {
        var newProductIds = new List<string>();

        var criteria = AbstractTypeFactory<ProductIndexedSearchCriteria>.TryCreateInstance();
        criteria.CatalogIds = [query.SourceCatalogId];
        criteria.Keyword = query.SourceCatalogQuery;
        criteria.Take = BatchSize;

        await foreach (var searchResult in productIndexedSearchService.SearchBatchesAsync(criteria, cancellationToken))
        {
            foreach (var productId in searchResult.Items.Select(x => x.Id))
            {
                var existingLink = linksToRemove.FirstOrDefault(x => x.ListEntryId.EqualsIgnoreCase(productId));

                if (existingLink != null)
                {
                    // Keep existing link
                    linksToRemove.Remove(existingLink);
                }
                else
                {
                    newProductIds.Add(productId);

                    if (newProductIds.Count >= BatchSize)
                    {
                        await CreateLinks(category, newProductIds, cancellationToken);
                        newProductIds.Clear();
                    }
                }
            }
        }

        if (newProductIds.Count > 0)
        {
            await CreateLinks(category, newProductIds, cancellationToken);
        }
    }

    protected virtual async Task CreateLinks(Category category, IList<string> productIds, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var products = await itemService.GetAsync(productIds);

        if (products.Count == 0)
        {
            return;
        }

        foreach (var product in products)
        {
            var link = AbstractTypeFactory<CategoryLink>.TryCreateInstance();
            link.IsAutomatic = true;
            link.CategoryId = category.Id;
            link.CatalogId = category.CatalogId;

            product.Links.Add(link);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await itemService.SaveChangesAsync(products);
    }

    protected virtual async Task DeleteAutomaticLinks(IList<CategoryLink> links, ICancellationToken cancellationToken)
    {
        foreach (var linksBatch in links.Where(x => x.IsAutomatic).Paginate(BatchSize))
        {
            await DeleteLinksBatch(linksBatch, cancellationToken);
        }
    }

    protected virtual async Task DeleteLinksBatch(IList<CategoryLink> links, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var productIds = links.Select(x => x.EntryId).ToList();
        var products = await itemService.GetAsync(productIds);

        if (products.Count == 0)
        {
            return;
        }

        var categoryId = links.First().CategoryId;

        foreach (var product in products)
        {
            var link = product.Links.FirstOrDefault(x => x.TargetId == categoryId);

            if (link != null)
            {
                product.Links.Remove(link);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await itemService.SaveChangesAsync(products);
    }
}
