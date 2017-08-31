using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
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
                        if (string.IsNullOrEmpty(code))
                        {
                            code = Guid.NewGuid().ToString("N");
                        }
                        category = _categoryService.Create(new Category { Name = categoryName, Code = code, CatalogId = catalog.Id, ParentId = parentCategoryId });
                        //Raise notification each notifyCategorySizeLimit category
                        progressInfo.Description = $"Creating categories: {++progressInfo.ProcessedCount} created";
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

            var defaultFulfilmentCenterId = _commerceService.GetAllFulfillmentCenters().FirstOrDefault()?.Id;

            ICollection<Property> modifiedProperties;
            LoadProductDependencies(csvProducts, catalog, out modifiedProperties);
            MergeFromAlreadyExistProducts(csvProducts, catalog);

            foreach (var csvProduct in csvProducts)
            {
                //Try to detect and split single property value in to multiple values for multivalue properties 
                csvProduct.PropertyValues = TryToSplitMultivaluePropertyValues(csvProduct);
            }

            progressInfo.Description = "Saving property dictionary values...";
            progressCallback(progressInfo);
            _propertyService.Update(modifiedProperties.ToArray());

            var totalProductsCount = csvProducts.Count;
            //Order to save main products first then variations
            csvProducts = csvProducts.OrderBy(x => x.MainProductId != null).ToList();
            for (var i = 0; i < totalProductsCount; i += 10)
            {
                var products = csvProducts.Skip(i).Take(10).ToArray();
                try
                {
                    //Save main products first and then variations
                    _productService.Update(products.OfType<CatalogProduct>().ToArray());

                    //Set productId for dependent objects
                    foreach (var product in products)
                    {
                        if (product.Inventory.FulfillmentCenterId != null || defaultFulfilmentCenterId != null)
                        {
                            product.Inventory.ProductId = product.Id;
                            product.Inventory.FulfillmentCenterId = product.Inventory.FulfillmentCenterId ?? defaultFulfilmentCenterId;
                            product.Price.ProductId = product.Id;
                        }
                        else
                        {
                            product.Inventory = null;
                        }
                    }
                    var productIds = products.Select(x => x.Id).ToArray();
                    var existInventories = _inventoryService.GetProductsInventoryInfos(productIds).ToList();
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
                    foreach (var price in prices)
                    {
                        var existPrice = existPrices.FirstOrDefault(x => x.Currency.EqualsInvariant(price.Currency) && x.ProductId.EqualsInvariant(price.ProductId));
                        if (existPrice != null)
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
                        progressInfo.ProcessedCount += products.Length;
                        progressInfo.Description = $"Saving products: {progressInfo.ProcessedCount} of {progressInfo.TotalCount} created";
                        progressCallback(progressInfo);
                    }
                }
            }
        }

        private static List<PropertyValue> TryToSplitMultivaluePropertyValues(CsvProduct csvProduct)
        {
            var result = new List<PropertyValue>();
            //Try to split multivalues
            foreach (var propValue in csvProduct.PropertyValues)
            {
                if (propValue.Value != null && propValue.Property != null && propValue.Property.Multivalue)
                {
                    var values = propValue.Value.ToString().Split(',', ';');
                    foreach (var value in values)
                    {
                        var multiPropValue = (PropertyValue)propValue.Clone();
                        multiPropValue.Value = value;
                        result.Add(multiPropValue);
                    }
                }
                else
                {
                    result.Add(propValue);
                }
            }
            return result;
        }

        private void LoadProductDependencies(IList<CsvProduct> csvProducts, Catalog catalog, out ICollection<Property> modifiedProperties)
        {
            modifiedProperties = new List<Property>();
            var allCategoriesIds = csvProducts.Select(x => x.CategoryId).Distinct().ToArray();
            var categoriesMap = _categoryService.GetByIds(allCategoriesIds, CategoryResponseGroup.Full).ToDictionary(x => x.Id);
            var defaultLanguge = catalog.DefaultLanguage != null ? catalog.DefaultLanguage.LanguageCode : "en-US";

            foreach (var csvProduct in csvProducts)
            {
                csvProduct.Catalog = catalog;
                csvProduct.CatalogId = catalog.Id;
                if (csvProduct.CategoryId != null)
                {
                    csvProduct.Category = categoriesMap[csvProduct.CategoryId];
                }

                //Try to set parent relations
                //By id or code reference
                var parentProduct = csvProducts.FirstOrDefault(x => csvProduct.MainProductId != null && (x.Id == csvProduct.MainProductId || x.Code == csvProduct.MainProductId));
                csvProduct.MainProduct = parentProduct;
                csvProduct.MainProductId = parentProduct != null ? parentProduct.Id : null;

                if (string.IsNullOrEmpty(csvProduct.Code))
                {
                    csvProduct.Code = _skuGenerator.GenerateSku(csvProduct);
                }
                csvProduct.EditorialReview.LanguageCode = defaultLanguge;
                csvProduct.SeoInfo.LanguageCode = defaultLanguge;
                csvProduct.SeoInfo.SemanticUrl = string.IsNullOrEmpty(csvProduct.SeoInfo.SemanticUrl) ? csvProduct.Code : csvProduct.SeoInfo.SemanticUrl;

                //Properties inheritance
                csvProduct.Properties = (csvProduct.Category != null ? csvProduct.Category.Properties : csvProduct.Catalog.Properties).OrderBy(x => x.Name).ToList();
                foreach (var propertyValue in csvProduct.PropertyValues.ToArray())
                {
                    //Try to find property meta information
                    propertyValue.Property = csvProduct.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(propertyValue.PropertyName));
                    if (propertyValue.Property != null)
                    {
                        propertyValue.ValueType = propertyValue.Property.ValueType;
                        if (propertyValue.Property.Dictionary)
                        {
                            var dicValue = propertyValue.Property.DictionaryValues.FirstOrDefault(x => Equals(x.Value, propertyValue.Value));
                            if (dicValue == null)
                            {
                                dicValue = new PropertyDictionaryValue
                                {
                                    Alias = propertyValue.Value.ToString(),
                                    Value = propertyValue.Value.ToString(),
                                    Id = Guid.NewGuid().ToString()
                                };
                                //need to register modified property for future update
                                if (!modifiedProperties.Contains(propertyValue.Property))
                                {
                                    modifiedProperties.Add(propertyValue.Property);
                                }
                            }
                            propertyValue.ValueId = dicValue.Id;
                        }
                    }
                }
            }
        }

        // Merge importing products with already exist to prevent erasing existing data, import should only update or create data
        private void MergeFromAlreadyExistProducts(IList<CsvProduct> csvProducts, Catalog catalog)
        {
            var transientProducts = csvProducts.Where(x => x.IsTransient()).ToArray();
            var nonTransientProducts = csvProducts.Where(x => !x.IsTransient()).ToArray();

            // Load existing products
            var alreadyExistProducts = new List<CatalogProduct>();
            for (var i = 0; i < nonTransientProducts.Length; i += 50)
            {
                alreadyExistProducts.AddRange(_productService.GetByIds(nonTransientProducts.Skip(i).Take(50).Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge));
            }

            // Detect existing product by Code
            var transientProductsCodes = transientProducts.Select(x => x.Code).Where(x => x != null).Distinct().ToArray();

            using (var repository = _catalogRepositoryFactory())
            {
                var foundProducts = repository.Items.Where(x => x.CatalogId == catalog.Id && transientProductsCodes.Contains(x.Code)).Select(x => new { x.Id, x.Code }).ToArray();
                for (var i = 0; i < foundProducts.Length; i += 50)
                {
                    alreadyExistProducts.AddRange(_productService.GetByIds(foundProducts.Skip(i).Take(50).Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge));
                }
            }

            foreach (var csvProduct in csvProducts)
            {
                var existProduct = csvProduct.IsTransient() ? alreadyExistProducts.FirstOrDefault(x => x.Code.EqualsInvariant(csvProduct.Code)) : alreadyExistProducts.FirstOrDefault(x => x.Id == csvProduct.Id);
                if (existProduct != null)
                {
                    csvProduct.MergeFrom(existProduct);
                }
            }
        }
    }
}
