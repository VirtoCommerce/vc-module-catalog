using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ProductExportPagedDataSource : ExportPagedDataSource<ProductExportDataQuery, ProductSearchCriteria>
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IItemService _itemService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IBlobUrlResolver _blobUrlResolver;


        public ProductExportPagedDataSource(IBlobStorageProvider blobStorageProvider, IItemService itemService, ICatalogSearchService catalogSearchService, IBlobUrlResolver blobUrlResolver, ProductExportDataQuery dataQuery) : base(dataQuery)
        {
            _blobStorageProvider = blobStorageProvider;
            _itemService = itemService;
            _catalogSearchService = catalogSearchService;
            _blobUrlResolver = blobUrlResolver;
        }


        protected override ExportableSearchResult FetchData(ProductSearchCriteria searchCriteria)
        {
            var result = Array.Empty<CatalogProduct>();
            int totalCount = 0;

            var responseGroup = BuildResponseGroup();
            var productIds = searchCriteria.ObjectIds?.ToArray() ?? Array.Empty<string>();

            if (productIds.IsNullOrEmpty())
            {
                var catalogSearchCriteria = AbstractTypeFactory<SearchCriteria>.TryCreateInstance();
                catalogSearchCriteria.SearchInChildren = true;
                catalogSearchCriteria.CategoryIds = searchCriteria.CategoryIds;
                catalogSearchCriteria.CatalogIds = searchCriteria.CatalogIds;
                catalogSearchCriteria.SearchInVariations = searchCriteria.SearchInVariations;
                catalogSearchCriteria.ResponseGroup = SearchResponseGroup.WithProducts;
                catalogSearchCriteria.Skip = searchCriteria.Skip;
                catalogSearchCriteria.Take = searchCriteria.Take;

                var productSearchResult = _catalogSearchService.Search(catalogSearchCriteria);
                productIds = productSearchResult.Products.Select(x => x.Id).ToArray();
                totalCount = productSearchResult.ProductsTotalCount;
            }

            if (!productIds.IsNullOrEmpty())
            {
                result = _itemService.GetByIds(productIds, responseGroup);
                totalCount = result.Length;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.Contains("Images.BinaryData")))
            {
                result.LoadImages(_blobStorageProvider);
            }

            return new ExportableSearchResult()
            {
                Results = ToExportable(result).ToList(),
                TotalCount = totalCount
            };

        }

        protected virtual IEnumerable<IExportable> ToExportable(IEnumerable<ICloneable> objects)
        {
            var models = objects.Cast<CatalogProduct>();

            var exportableProducts = models.Select(x =>
            {
                var exportableProduct = AbstractTypeFactory<ExportableProduct>.TryCreateInstance().FromModel(x);
                var imageUrl = x?.Images?.FirstOrDefault()?.Url;

                exportableProduct.ImageUrl = imageUrl != null ? _blobUrlResolver.GetAbsoluteUrl(imageUrl) : null;
                return exportableProduct;
            });

            return exportableProducts;
        }

        protected override ProductSearchCriteria BuildSearchCriteria(ProductExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.SearchInVariations = exportDataQuery.SearchInVariations;
            result.CatalogIds = exportDataQuery.CatalogIds;
            result.CategoryIds = exportDataQuery.CategoryIds;

            return result;
        }

        private ItemResponseGroup BuildResponseGroup()
        {
            var result = ItemResponseGroup.ItemInfo;

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Assets) + ".")))
            {
                result |= ItemResponseGroup.ItemAssets;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Properties) + ".")))
            {
                result |= ItemResponseGroup.ItemProperties;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Associations) + ".")))
            {
                result |= ItemResponseGroup.ItemAssociations;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Variations) + ".")))
            {
                result |= ItemResponseGroup.Variations;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.SeoInfos) + ".")))
            {
                result |= ItemResponseGroup.Seo;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Links) + ".")))
            {
                result |= ItemResponseGroup.Links;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.ReferencedAssociations) + ".")))
            {
                result |= ItemResponseGroup.ReferencedAssociations;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Outlines) + ".")))
            {
                result |= ItemResponseGroup.Outlines;
            }

            if (DataQuery.IncludedProperties.Any(x => x.FullName.StartsWith(nameof(CatalogProduct.Reviews) + ".")))
            {
                result |= ItemResponseGroup.ItemEditorialReviews;
            }

            return result;
        }
    }
}
