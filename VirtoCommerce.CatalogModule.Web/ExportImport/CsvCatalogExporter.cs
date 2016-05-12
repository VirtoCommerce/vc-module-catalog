using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogModule.Web.ExportImport
{
    public sealed class CsvCatalogExporter
    {
        private readonly ICatalogSearchService _searchService;
        private readonly IItemService _productService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CsvCatalogExporter(ICatalogSearchService catalogSearchService, IItemService productService,
                                  IPricingService pricingService, IInventoryService inventoryService, IBlobUrlResolver blobUrlResolver)
        {
            _searchService = catalogSearchService;
            _productService = productService;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _blobUrlResolver = blobUrlResolver;
        }

        public void DoExport(Stream outStream, CsvExportInfo exportInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var prodgressInfo = new ExportImportProgressInfo
            {
                Description = "loading products..."
            };

            var streamWriter = new StreamWriter(outStream, Encoding.UTF8, 1024, true) { AutoFlush = true };
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                //Notification
                progressCallback(prodgressInfo);

                //Load all products to export
                var products = LoadProducts(exportInfo.CatalogId, exportInfo.CategoryIds, exportInfo.ProductIds);
                var allProductIds = products.Select(x => x.Id).ToArray();

                //Load prices for products
                prodgressInfo.Description = "loading prices...";
                progressCallback(prodgressInfo);

                var priceEvalContext = new PriceEvaluationContext
                {
                    ProductIds = allProductIds,
                    PricelistIds = exportInfo.PriceListId == null ? null : new[] { exportInfo.PriceListId },
                    Currency = exportInfo.Currency
                };
                var allProductPrices = _pricingService.EvaluateProductPrices(priceEvalContext).ToList();


                //Load inventories
                prodgressInfo.Description = "loading inventory information...";
                progressCallback(prodgressInfo);

                var allProductInventories = _inventoryService.GetProductsInventoryInfos(allProductIds).Where(x => exportInfo.FulfilmentCenterId == null || x.FulfillmentCenterId == exportInfo.FulfilmentCenterId).ToList();


                //Export configuration
                exportInfo.Configuration.PropertyCsvColumns = products.SelectMany(x => x.PropertyValues).Select(x => x.PropertyName).Distinct().ToArray();

                csvWriter.Configuration.Delimiter = exportInfo.Configuration.Delimiter;
                csvWriter.Configuration.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));

                //Write header
                csvWriter.WriteHeader<CsvProduct>();

                prodgressInfo.TotalCount = products.Count;
                var notifyProductSizeLimit = 50;
                var counter = 0;
                foreach (var product in products)
                {
                    try
                    {
                        var csvProduct = new CsvProduct(product, _blobUrlResolver, allProductPrices.FirstOrDefault(x => x.ProductId == product.Id), allProductInventories.FirstOrDefault(x => x.ProductId == product.Id));
                        csvWriter.WriteRecord(csvProduct);
                    }
                    catch (Exception ex)
                    {
                        prodgressInfo.Errors.Add(ex.ToString());
                        progressCallback(prodgressInfo);
                    }

                    //Raise notification each notifyProductSizeLimit products
                    counter++;
                    prodgressInfo.ProcessedCount = counter;
                    prodgressInfo.Description = string.Format("{0} of {1} products processed", prodgressInfo.ProcessedCount, prodgressInfo.TotalCount);
                    if (counter % notifyProductSizeLimit == 0 || counter == prodgressInfo.TotalCount)
                    {
                        progressCallback(prodgressInfo);
                    }
                }
            }
        }


        private List<CatalogProduct> LoadProducts(string catalogId, string[] exportedCategories, string[] exportedProducts)
        {
            var retVal = new List<CatalogProduct>();

            var productIds = new List<string>();
            if (exportedProducts != null)
            {
                productIds = exportedProducts.ToList();
            }
            if (exportedCategories != null && exportedCategories.Any())
            {
                foreach (var categoryId in exportedCategories)
                {
                    var result = _searchService.Search(new SearchCriteria { CatalogId = catalogId, CategoryId = categoryId, Skip = 0, Take = int.MaxValue, ResponseGroup = SearchResponseGroup.WithProducts | SearchResponseGroup.WithCategories });
                    productIds.AddRange(result.Products.Select(x => x.Id));
                    if (result.Categories != null && result.Categories.Any())
                    {
                        retVal.AddRange(LoadProducts(catalogId, result.Categories.Select(x => x.Id).ToArray(), null));
                    }
                }
            }

            if ((exportedCategories == null || !exportedCategories.Any()) && (exportedProducts == null || !exportedProducts.Any()))
            {
                var result = _searchService.Search(new SearchCriteria { CatalogId = catalogId, SearchInChildren = true, Skip = 0, Take = int.MaxValue, ResponseGroup = SearchResponseGroup.WithProducts });
                productIds = result.Products.Select(x => x.Id).ToList();
            }

            var products = _productService.GetByIds(productIds.Distinct().ToArray(), ItemResponseGroup.ItemLarge);
            foreach (var product in products)
            {
                retVal.Add(product);
                if (product.Variations != null)
                {
                    retVal.AddRange(product.Variations);
                }
            }

            return retVal;
        }
    }
}
