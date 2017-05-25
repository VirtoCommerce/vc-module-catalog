using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogModule.Web.ExportImport
{
    public sealed class CsvCatalogImporter
    {
        private readonly char[] _categoryDelimiters = { '/', '|', '\\', '>' };
        private readonly ICatalogService _catalogService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _productService;
        private readonly ISkuGenerator _skuGenerator;
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly IInventoryService _inventoryService;
        private readonly ICommerceService _commerceService;
        private readonly IPropertyService _propertyService;
        private readonly ICatalogSearchService _searchService;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly object _lockObject = new object();

        public CsvCatalogImporter(ICatalogService catalogService, ICategoryService categoryService, IItemService productService,
                                  ISkuGenerator skuGenerator,
                                  IPricingService pricingService, IInventoryService inventoryService, ICommerceService commerceService,
                                  IPropertyService propertyService, ICatalogSearchService searchService, Func<ICatalogRepository> catalogRepositoryFactory, IPricingSearchService pricingSearchService)
        {
            _catalogService = catalogService;
            _categoryService = categoryService;
            _productService = productService;
            _skuGenerator = skuGenerator;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _commerceService = commerceService;
            _propertyService = propertyService;
            _searchService = searchService;
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _pricingSearchService = pricingSearchService;
        }

        public void DoImport(Stream inputStream, CsvImportInfo importInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            var csvProducts = new List<CsvProduct>();

            var progressInfo = new ExportImportProgressInfo
            {
                Description = "Reading products from csv..."
            };
            progressCallback(progressInfo);

            using (var reader = new CsvReader(new StreamReader(inputStream)))
            {
                reader.Configuration.Delimiter = importInfo.Configuration.Delimiter;
                reader.Configuration.RegisterClassMap(new CsvProductMap(importInfo.Configuration));
                reader.Configuration.WillThrowOnMissingField = false;

                while (reader.Read())
                {
                    try
                    {
                        var csvProduct = reader.GetRecord<CsvProduct>();
                        csvProducts.Add(csvProduct);
                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                        if (ex.Data.Contains("CsvHelper"))
                        {
                            error += ex.Data["CsvHelper"];
                        }
                        progressInfo.Errors.Add(error);
                        progressCallback(progressInfo);
                    }
                }
            }

            var catalog = _catalogService.GetById(importInfo.CatalogId);

            SaveCategoryTree(catalog, csvProducts, progressInfo, progressCallback);
            SaveProducts(catalog, csvProducts, progressInfo, progressCallback);
        }


        private void SaveCategoryTree(Catalog catalog, IEnumerable<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            progressInfo.ProcessedCount = 0;
            var cachedCategoryMap = new Dictionary<string, Category>();

            foreach (var csvProduct in csvProducts.Where(x => x.Category != null && !string.IsNullOrEmpty(x.Category.Path)))
            {
                var outline = "";
                var productCategoryNames = csvProduct.Category.Path.Split(_categoryDelimiters);
                string parentCategoryId = null;
                foreach (var categoryName in productCategoryNames)
                {
                    outline += "\\" + categoryName;
                    Category category;
                    if (!cachedCategoryMap.TryGetValue(outline, out category))
                    {
                        var searchCriteria = new SearchCriteria
                        {
                            CatalogId = catalog.Id,
                            CategoryId = parentCategoryId,
                            ResponseGroup = SearchResponseGroup.WithCategories
                        };
                        category = _searchService.Search(searchCriteria).Categories.FirstOrDefault(x => x.Name == categoryName);
                    }

                    if (category == null)
                    {
                        var code = categoryName.GenerateSlug();
                        if(string.IsNullOrEmpty(code))
                        {
                            code = Guid.NewGuid().ToString("N");
                        }
                        category = _categoryService.Create(new Category() { Name = categoryName, Code = code, CatalogId = catalog.Id, ParentId = parentCategoryId });
                        //Raise notification each notifyCategorySizeLimit category
                        progressInfo.Description = string.Format("Creating categories: {0} created", ++progressInfo.ProcessedCount);
                        progressCallback(progressInfo);
                    }
                    csvProduct.CategoryId = category.Id;
                    csvProduct.Category = category;
                    parentCategoryId = category.Id;
                    cachedCategoryMap[outline] = category;
                }
            }
        }

        private void SaveProducts(Catalog catalog, List<CsvProduct> csvProducts, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            progressInfo.ProcessedCount = 0;
            progressInfo.TotalCount = csvProducts.Count;

            var defaultFulfilmentCenter = _commerceService.GetAllFulfillmentCenters().FirstOrDefault();
            DetectParents(csvProducts);

            //Detect already exist product by Code
            using (var repository = _catalogRepositoryFactory())
            {
                var codes = csvProducts.Where(x => x.IsTransient()).Select(x => x.Code).Where(x => x != null).Distinct().ToArray();
                var existProducts = repository.Items.Where(x => x.CatalogId == catalog.Id && codes.Contains(x.Code)).Select(x => new { Id = x.Id, Code = x.Code }).ToArray();
                foreach (var existProduct in existProducts)
                {
                    var product = csvProducts.FirstOrDefault(x => x.Code == existProduct.Code);
                    if (product != null)
                    {
                        product.Id = product.Id;
                    }
                }
            }

            var categoriesIds = csvProducts.Where(x => x.CategoryId != null).Select(x => x.CategoryId).Distinct().ToArray();
            var categpories = _categoryService.GetByIds(categoriesIds, CategoryResponseGroup.WithProperties);

            var defaultLanguge = catalog.DefaultLanguage != null ? catalog.DefaultLanguage.LanguageCode : "EN-US";
            var changedProperties = new List<Property>();
            foreach (var csvProduct in csvProducts)
            {
                csvProduct.CatalogId = catalog.Id;
                if (string.IsNullOrEmpty(csvProduct.Code))
                {
                    csvProduct.Code = _skuGenerator.GenerateSku(csvProduct);
                }
                //Set a parent relations
                if (csvProduct.MainProductId == null && csvProduct.MainProduct != null)
                {
                    csvProduct.MainProductId = csvProduct.MainProduct.Id;
                }
                csvProduct.EditorialReview.LanguageCode = defaultLanguge;
                csvProduct.SeoInfo.LanguageCode = defaultLanguge;
                csvProduct.SeoInfo.SemanticUrl = string.IsNullOrEmpty(csvProduct.SeoInfo.SemanticUrl) ? csvProduct.Code : csvProduct.SeoInfo.SemanticUrl;

                var properties = catalog.Properties;
                if (csvProduct.CategoryId != null)
                {
                    var category = categpories.FirstOrDefault(x => x.Id == csvProduct.CategoryId);
                    if (category != null)
                    {
                        properties = category.Properties;
                    }
                }

                //Try to fill properties meta information for values
                foreach (var propertyValue in csvProduct.PropertyValues)
                {
                    if (propertyValue.Value != null)
                    {
                        var property = properties.FirstOrDefault(x => string.Equals(x.Name, propertyValue.PropertyName));
                        if (property != null)
                        {
                            propertyValue.ValueType = property.ValueType;
                            if (property.Dictionary)
                            {
                                var dicValue = property.DictionaryValues.FirstOrDefault(x => Equals(x.Value, propertyValue.Value));
                                if (dicValue == null)
                                {
                                    dicValue = new PropertyDictionaryValue
                                    {
                                        Alias = propertyValue.Value.ToString(),
                                        Value = propertyValue.Value.ToString(),
                                        Id = Guid.NewGuid().ToString()
                                    };
                                    property.DictionaryValues.Add(dicValue);
                                    if(!changedProperties.Contains(property))
                                    {
                                        changedProperties.Add(property);
                                    }
                                }
                                propertyValue.ValueId = dicValue.Id;
                            }
                        }
                    }
                }             
            }

            progressInfo.Description = string.Format("Saving property dictionary values...");
            progressCallback(progressInfo);
            _propertyService.Update(changedProperties.ToArray());

            var totalProductsCount = csvProducts.Count();
            //Order to save main products first then variations
            csvProducts = csvProducts.OrderBy(x => x.MainProductId != null).ToList();
            for (int i = 0; i< totalProductsCount; i += 50)
            {
                var products = csvProducts.Skip(i).Take(50);
                try
                {
                    //Save main products first and then variations
                    _productService.Update(products.ToArray());

                    //Set productId for dependent objects
                    foreach (var product in products)
                    {
                        if (defaultFulfilmentCenter != null || product.Inventory.FulfillmentCenterId != null)
                        {
                            product.Inventory.ProductId = product.Id;
                            product.Inventory.FulfillmentCenterId = product.Inventory.FulfillmentCenterId ?? defaultFulfilmentCenter.Id;
                            product.Price.ProductId = product.Id;
                        }
                        else
                        {
                            product.Inventory = null;
                        }
                    }
                    var productIds = products.Select(x => x.Id).ToArray();
                    var existInventories = _inventoryService.GetProductsInventoryInfos(productIds);
                    var inventories = products.Where(x => x.Inventory != null).Select(x => x.Inventory).ToArray();
                    foreach (var inventory in inventories)
                    {
                        var exitsInventory = existInventories.FirstOrDefault(x => x.ProductId == inventory.ProductId && x.FulfillmentCenterId == inventory.FulfillmentCenterId);
                        if (exitsInventory != null)
                        {
                            inventory.InjectFrom(exitsInventory);
                        }
                    }
                    _inventoryService.UpsertInventories(inventories);

                    //We do not have information about concrete price list id and therefore select first product price then
                    var existPrices = _pricingSearchService.SearchPrices(new Domain.Pricing.Model.Search.PricesSearchCriteria { ProductIds = productIds }).Results;
                    var prices = products.Where(x => x.Price != null && x.Price.EffectiveValue > 0).Select(x => x.Price).ToArray();
                    foreach(var price in prices)
                    {
                        var existPrice = existPrices.FirstOrDefault(x => x.Currency.EqualsInvariant(price.Currency) && x.ProductId.EqualsInvariant(price.ProductId));
                        if(existPrice != null)
                        {
                            price.InjectFrom(existPrice);
                        }
                    }
                    _pricingService.SavePrices(prices);
                }
                catch (Exception ex)
                {
                    lock (_lockObject)
                    {
                        progressInfo.Errors.Add(ex.ToString());
                        progressCallback(progressInfo);
                    }
                }
                finally
                {
                    lock (_lockObject)
                    {
                        //Raise notification
                        progressInfo.ProcessedCount += products.Count();
                        progressInfo.Description = string.Format("Saving products: {0} of {1} created", progressInfo.ProcessedCount, progressInfo.TotalCount);
                        progressCallback(progressInfo);
                    }
                }
            }
        }

        private void DetectParents(List<CsvProduct> csvProducts)
        {
            foreach (var csvProduct in csvProducts)
            {
                //Try to set parent relations
                //By id or code reference
                var parentProduct = csvProducts.FirstOrDefault(x => csvProduct.MainProductId != null && (x.Id == csvProduct.MainProductId || x.Code == csvProduct.MainProductId));
                csvProduct.MainProduct = parentProduct;
                csvProduct.MainProductId = parentProduct != null ? parentProduct.Id : null;
            }
        }
    }
}
