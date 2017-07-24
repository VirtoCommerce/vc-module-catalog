using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Data.Common;

namespace VirtoCommerce.CatalogModule.Web.ExportImport
{
    public sealed class CatalogExportImport
    {
        private readonly ICatalogService _catalogService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IPropertyService _propertyService;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IAssociationService _associationService;
        private const int _batchSize = 50;
        private readonly JsonSerializer _serializer;


        public CatalogExportImport(ICatalogSearchService catalogSearchService,
            ICatalogService catalogService, ICategoryService categoryService, IItemService itemService,
            IPropertyService propertyService, IBlobStorageProvider blobStorageProvider, IAssociationService associationService)
        {
            _blobStorageProvider = blobStorageProvider;
            _catalogSearchService = catalogSearchService;
            _catalogService = catalogService;
            _categoryService = categoryService;
            _itemService = itemService;
            _propertyService = propertyService;
            _associationService = associationService;

            _serializer = new JsonSerializer();
            _serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializer.Formatting = Formatting.Indented;
            _serializer.NullValueHandling = NullValueHandling.Ignore;
        }

        #region Export/Import methods
        public void DoExport(Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (StreamWriter sw = new StreamWriter(outStream, Encoding.UTF8))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                ExportCatalogs(writer, _serializer, manifest, progressInfo, progressCallback);
                ExportCategories(writer, _serializer, manifest, progressInfo, progressCallback);
                ExportProperties(writer, _serializer, manifest, progressInfo, progressCallback);
                ExportProducts(writer, _serializer, manifest, progressInfo, progressCallback);

                writer.WriteEndObject();
                writer.Flush();
            }
        }


        public void DoImport(Stream stream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();
            var productsTotalCount = 0;
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString() == "Catalogs")
                        {
                            reader.Read();
                            var catalogs = _serializer.Deserialize<Catalog[]>(reader);
                            progressInfo.Description = $"{ catalogs.Count() } catalogs importing...";
                            progressCallback(progressInfo);
                            _catalogService.Update(catalogs);
                        }
                        else if (reader.Value.ToString() == "Categories")
                        {
                            reader.Read();
                            var categories = _serializer.Deserialize<Category[]>(reader);
                            progressInfo.Description = $"{ categories.Count() } categories importing...";
                            progressCallback(progressInfo);
                            _categoryService.Update(categories);
                            if (manifest.HandleBinaryData)
                            {
                                ImportImages(categories);
                            }
                        }
                        else if (reader.Value.ToString() == "Properties")
                        {
                            reader.Read();
                            var properties = _serializer.Deserialize<Property[]>(reader);
                            progressInfo.Description = $"{ properties.Count() } properties importing...";
                            progressCallback(progressInfo);
                            _propertyService.Update(properties);
                        }
                        else if (reader.Value.ToString() == "ProductsTotalCount")
                        {
                            productsTotalCount = reader.ReadAsInt32() ?? 0;
                        }
                        else if (reader.Value.ToString() == "Products")
                        {
                            reader.Read();

                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                reader.Read();

                                var associationBackupMap = new Dictionary<string, ICollection<ProductAssociation>>();
                                var products = new List<CatalogProduct>();
                                var productsCount = 0;
                                //Import products
                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var product = _serializer.Deserialize<CatalogProduct>(reader);
                                    //Do not save associations withing product to prevent dependency conflicts in db
                                    //we will save separateley after product import
                                    if (!product.Associations.IsNullOrEmpty())
                                    {
                                        associationBackupMap[product.Id] = product.Associations;
                                    }
                                    product.Associations = null;
                                    products.Add(product);
                                    productsCount++;
                                    reader.Read();
                                    if (productsCount % _batchSize == 0 || reader.TokenType == JsonToken.EndArray)
                                    {
                                        _itemService.Update(products.ToArray());
                                        if (manifest.HandleBinaryData)
                                        {
                                            ImportImages(products.ToArray());
                                        }

                                        products.Clear();
                                        if (productsTotalCount > 0)
                                        {
                                            progressInfo.Description = $"{ productsCount } of { productsTotalCount } products imported";
                                        }
                                        else
                                        {
                                            progressInfo.Description = $"{ productsCount } products imported";
                                        }
                                        progressCallback(progressInfo);
                                    }          
                                }
                                //Import products associations separately to avoid DB constrain violation
                                var totalProductsWithAssociationsCount = associationBackupMap.Count();
                                progressInfo.Description = $"{ totalProductsWithAssociationsCount } products associations importing...";
                                progressCallback(progressInfo);
                                for (int i = 0; i < totalProductsWithAssociationsCount; i += _batchSize)
                                {
                                    var fakeProducts = new List<CatalogProduct>();
                                    foreach (var pair in associationBackupMap.Skip(i).Take(_batchSize))
                                    {
                                        var fakeProduct = new CatalogProduct
                                        {
                                            Id = pair.Key,
                                            Associations = pair.Value
                                        };
                                        fakeProducts.Add(fakeProduct);
                                    }
                                    _associationService.SaveChanges(fakeProducts.ToArray());
                                    progressInfo.Description = $"{ Math.Min(totalProductsWithAssociationsCount, i + _batchSize) } of { totalProductsWithAssociationsCount } products associations imported";
                                    progressCallback(progressInfo);
                                }
                            }
                        }
                    }
                }
            }
        } 
        #endregion

        private void ExportCatalogs(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            //Catalogs
            progressInfo.Description = string.Format("Catalogs exporting...");
            progressCallback(progressInfo);

            var catalogs = _catalogService.GetCatalogsList().ToArray();

            writer.WritePropertyName("Catalogs");
            writer.WriteStartArray();
            //Reset some props to descrease resulting json size
            foreach (var catalog in catalogs)
            {
                ResetRedundantReferences(catalog);
                serializer.Serialize(writer, catalog);
            }
            writer.WriteEndArray();


            progressInfo.Description = $"{ catalogs.Count() } catalogs exported";
            progressCallback(progressInfo);
        }

        private void ExportCategories(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            //Categories
            progressInfo.Description = string.Format("Categories exporting...");
            progressCallback(progressInfo);

            var categorySearchCriteria = new SearchCriteria { WithHidden = true, Skip = 0, Take = 0, ResponseGroup = SearchResponseGroup.WithCategories };
            var categoriesSearchResult = _catalogSearchService.Search(categorySearchCriteria);
            var categories = _categoryService.GetByIds(categoriesSearchResult.Categories.Select(x => x.Id).ToArray(), CategoryResponseGroup.Full);
            if (manifest.HandleBinaryData)
            {
                LoadImages(categories);
            }

            writer.WritePropertyName("Categories");
            writer.WriteStartArray();
            //reset some properties to decrease resultin JSON size
            foreach (var category in categories)
            {
                ResetRedundantReferences(category);
                serializer.Serialize(writer, category);
            }
            writer.WriteEndArray();

            progressInfo.Description = $"{ categoriesSearchResult.Categories.Count } categories exported";
            progressCallback(progressInfo);
        }

        private void ExportProperties(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            //Properties
            progressInfo.Description = "Properties exporting...";
            progressCallback(progressInfo);

            var properties = _propertyService.GetAllProperties();
            writer.WritePropertyName("Properties");
            writer.WriteStartArray();
            //Reset some props to descrease resulting json size
            foreach (var property in properties)
            {
                ResetRedundantReferences(property);
                serializer.Serialize(writer, property);
            }
            writer.WriteEndArray();

            progressInfo.Description = $"{ properties.Count() } properties exported";
            progressCallback(progressInfo);
        }

        private void ExportProducts(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            //Products
            progressInfo.Description = string.Format("Products exporting...");
            progressCallback(progressInfo);

            var productSearchCriteria = new SearchCriteria { WithHidden = true, Take = 0, Skip = 0, ResponseGroup = SearchResponseGroup.WithProducts };
            var totalProductCount = _catalogSearchService.Search(productSearchCriteria).ProductsTotalCount;
            writer.WritePropertyName("ProductsTotalCount");
            writer.WriteValue(totalProductCount);

            writer.WritePropertyName("Products");
            writer.WriteStartArray();
            for (int i = 0; i < totalProductCount; i += _batchSize)
            {
                var searchResponse = _catalogSearchService.Search(new SearchCriteria { WithHidden = true, Take = _batchSize, Skip = i, ResponseGroup = SearchResponseGroup.WithProducts });

                var products = _itemService.GetByIds(searchResponse.Products.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemLarge);
                if (manifest.HandleBinaryData)
                {
                    LoadImages(products);
                }
                foreach (var product in products)
                {
                    ResetRedundantReferences(product);
                    serializer.Serialize(writer, product);
                }
                writer.Flush();
                progressInfo.Description = $"{ Math.Min(totalProductCount, i + _batchSize) } of { totalProductCount } products exported";
                progressCallback(progressInfo);
            }
            writer.WriteEndArray();
        }

        //Remove redundant references to reduce resulting JSON size
        private static void ResetRedundantReferences(object entity)
        {
            var product = entity as CatalogProduct;
            var category = entity as Category;
            var catalog = entity as Catalog;
            var asscociation = entity as ProductAssociation;
            var property = entity as Property;
            var propertyValue = entity as PropertyValue;

            if (propertyValue != null)
            {
                propertyValue.Property = null;
            }

            if (asscociation != null)
            {
                asscociation.AssociatedObject = null;
            }

            if (catalog != null)
            {
                catalog.Properties = null;
                foreach (var lang in catalog.Languages)
                {
                    lang.Catalog = null;
                }
            }

            if (category != null)
            {
                category.Catalog = null;
                category.Properties = null;
                category.Children = null;
                category.Parents = null;
                category.Outlines = null;
                if (category.PropertyValues != null)
                {
                    foreach (var propvalue in category.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
            }

            if (property != null)
            {
                property.Catalog = null;
                property.Category = null;
            }

            if (product != null)
            {
                product.Catalog = null;
                product.Category = null;
                product.Properties = null;
                product.MainProduct = null;
                product.Outlines = null;
                if (product.PropertyValues != null)
                {
                    foreach (var propvalue in product.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
                if (product.Associations != null)
                {
                    foreach (var association in product.Associations)
                    {
                        ResetRedundantReferences(association);
                    }
                }
                if (product.Variations != null)
                {
                    foreach (var variation in product.Variations)
                    {
                        ResetRedundantReferences(variation);
                    }
                }
            }
        }

        private void LoadImages(IHasImages[] haveImagesObjects)
        {
            var allImages = haveImagesObjects.SelectMany(x => x.GetFlatObjectsListWithInterface<IHasImages>())
                                             .SelectMany(x => x.Images).ToArray();
            foreach (var image in allImages)
            {
                using (var stream = _blobStorageProvider.OpenRead(image.Url))
                {
                    image.BinaryData = stream.ReadFully();
                }
            }
        }

        private void ImportImages(IHasImages[] haveImagesObjects)
        {

            var allImages = haveImagesObjects.SelectMany(x => x.GetFlatObjectsListWithInterface<IHasImages>())
                                       .SelectMany(x => x.Images).ToArray();
            foreach (var image in allImages.Where(x => x.BinaryData != null))
            {
                //do not save images with external url
                if (image.Url != null && !image.Url.IsAbsoluteUrl())
                {
                    using (var sourceStream = new MemoryStream(image.BinaryData))
                    using (var targetStream = _blobStorageProvider.OpenWrite(image.Url))
                    {
                        sourceStream.CopyTo(targetStream);
                    }
                }
            }
        }

    }
}
