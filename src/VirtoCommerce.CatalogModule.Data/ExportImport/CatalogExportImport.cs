using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogExportImport
    {
        private readonly ICatalogService _catalogService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IProductSearchService _productSearchService;
        private readonly ICategorySearchService _categorySearchService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IPropertyService _propertyService;
        private readonly IPropertySearchService _propertySearchService;
        private readonly IPropertyDictionaryItemSearchService _propertyDictionarySearchService;
        private readonly IPropertyDictionaryItemService _propertyDictionaryService;
        private readonly JsonSerializer _jsonSerializer;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IAssociationService _associationService;

        private readonly int _batchSize = 50;

        public CatalogExportImport(ICatalogService catalogService, ICatalogSearchService catalogSearchService, IProductSearchService productSearchService, ICategorySearchService categorySearchService, ICategoryService categoryService,
                                  IItemService itemService, IPropertyService propertyService, IPropertySearchService propertySearchService, IPropertyDictionaryItemSearchService propertyDictionarySearchService,
                                  IPropertyDictionaryItemService propertyDictionaryService, JsonSerializer jsonSerializer, IBlobStorageProvider blobStorageProvider, IAssociationService associationService)
        {
            _catalogService = catalogService;
            _productSearchService = productSearchService;
            _categorySearchService = categorySearchService;
            _categoryService = categoryService;
            _itemService = itemService;
            _propertyService = propertyService;
            _propertySearchService = propertySearchService;
            _propertyDictionarySearchService = propertyDictionarySearchService;
            _propertyDictionaryService = propertyDictionaryService;
            _jsonSerializer = jsonSerializer;
            _blobStorageProvider = blobStorageProvider;
            _associationService = associationService;
            _catalogSearchService = catalogSearchService;
        }

        public async Task DoExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(outStream))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                #region Export properties

                progressInfo.Description = "Properties exporting...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Properties");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                {
                    var searchResult = await _propertySearchService.SearchPropertiesAsync(new PropertySearchCriteria { Skip = skip, Take = take });
                    foreach (var item in searchResult.Results)
                    {
                        ResetRedundantReferences(item);
                    }
                    return (GenericSearchResult<Property>)searchResult;
                }
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} properties have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion Export properties

                #region Export propertyDictionaryItems

                progressInfo.Description = "PropertyDictionaryItems exporting...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("PropertyDictionaryItems");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                    (GenericSearchResult<PropertyDictionaryItem>)await _propertyDictionarySearchService.SearchAsync(new PropertyDictionaryItemSearchCriteria { Skip = skip, Take = take }, clone: true)
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} property dictionary items have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion Export propertyDictionaryItems

                #region Export catalogs

                progressInfo.Description = "Catalogs exporting...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Catalogs");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                    (GenericSearchResult<Catalog>)await _catalogSearchService.SearchNoCloneAsync(new CatalogSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} catalogs have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion Export catalogs

                #region Export categories

                progressInfo.Description = "Categories exporting...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Categories");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                {
                    var searchResult = await _categorySearchService.SearchAsync(new CategorySearchCriteria { Skip = skip, Take = take });
                    LoadImages(searchResult.Results.OfType<IHasImages>().ToArray(), progressInfo, options.HandleBinaryData);
                    foreach (var item in searchResult.Results)
                    {
                        ResetRedundantReferences(item);
                    }

                    return (GenericSearchResult<Category>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} Categories have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion Export categories

                #region Export products

                progressInfo.Description = "Products exporting...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Products");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                {
                    var searchResult = await _productSearchService.SearchAsync(new ProductSearchCriteria { Skip = skip, Take = take, ResponseGroup = ItemResponseGroup.Full.ToString() });
                    LoadImages(searchResult.Results.OfType<IHasImages>().ToArray(), progressInfo, options.HandleBinaryData);
                    foreach (var item in searchResult.Results)
                    {
                        ResetRedundantReferences(item);
                    }
                    return (GenericSearchResult<CatalogProduct>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} Products have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion Export products

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();
            var propertiesWithForeignKeys = new List<Property>();

            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        continue;
                    }

                    switch (reader.Value.ToString())
                    {
                        case "Properties":
                            await ImportPropertiesAsync(reader, propertiesWithForeignKeys, progressInfo, progressCallback, cancellationToken);
                            break;

                        case "PropertyDictionaryItems":
                            await ImportPropertyDictionaryItemsAsync(reader, progressInfo, progressCallback, cancellationToken);
                            break;

                        case "Catalogs":
                            await ImportCatalogsAsync(reader, progressInfo, progressCallback, cancellationToken);
                            break;

                        case "Categories":
                            await ImportCategoriesAsync(reader, progressInfo, progressCallback, cancellationToken);
                            break;

                        case "Products":
                            await ImportProductsAsync(reader, options, progressInfo, progressCallback, cancellationToken);
                            break;

                        default:
                            continue;
                    }
                }
            }

            //Update property associations after all required data are saved (Catalogs and Categories)
            if (propertiesWithForeignKeys.Count > 0)
            {
                progressInfo.Description = $"Updating {propertiesWithForeignKeys.Count} property associations…";
                progressCallback(progressInfo);

                var totalCount = propertiesWithForeignKeys.Count;
                for (var i = 0; i < totalCount; i += _batchSize)
                {
                    await _propertyService.SaveChangesAsync(propertiesWithForeignKeys.Skip(i).Take(_batchSize).ToArray());
                    progressInfo.Description = $"{Math.Min(totalCount, i + _batchSize)} of {totalCount} property associations updated.";
                    progressCallback(progressInfo);
                }
            }
        }

        private async Task ImportCatalogsAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await reader.DeserializeArrayWithPagingAsync<Catalog>(_jsonSerializer, _batchSize, async catalogs =>
                {
                    foreach (var catalog in catalogs)
                    {
                        if (catalog.SeoInfos == null || !catalog.SeoInfos.Any())
                        {
                            var defaultLanguage = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                            var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                            seoInfo.LanguageCode = defaultLanguage;
                            seoInfo.SemanticUrl = "catalog";
                            seoInfo.PageTitle = "Catalog";
                            catalog.SeoInfos = [seoInfo];
                        }

                        foreach (var seoInfo in catalog.SeoInfos)
                        {
                            seoInfo.SemanticUrl ??= "catalog";
                            seoInfo.PageTitle ??= "Catalog";
                        }
                    }

                    await _catalogService.SaveChangesAsync(catalogs);
                },
                processedCount =>
                {
                    progressInfo.Description = $"{processedCount} catalogs have been imported";
                    progressCallback(progressInfo);
                }, cancellationToken);
        }

        private async Task ImportCategoriesAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            var processedCount = 0;

            var categoriesByHierarchyLevel = new Dictionary<int, IList<Category>>();
            var categoryLinks = new List<CategoryLink>();

            await reader.DeserializeArrayWithPagingAsync<Category>(_jsonSerializer, _batchSize, async items =>
            {
                var categories = new List<Category>();

                foreach (var category in items)
                {
                    var slugUrl = category.Name.GenerateSlug();

                    if (category.SeoInfos == null || !category.SeoInfos.Any())
                    {
                        if (!string.IsNullOrEmpty(slugUrl))
                        {
                            var catalog = await _catalogService.GetNoCloneAsync(category.CatalogId);
                            var defaultLanguage = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                            var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                            seoInfo.LanguageCode = defaultLanguage;
                            seoInfo.SemanticUrl = slugUrl;
                            seoInfo.PageTitle = category.Name.SoftTruncate(70);
                            category.SeoInfos = [seoInfo];
                        }
                    }

                    foreach (var seoInfo in category.SeoInfos)
                    {
                        if (string.IsNullOrEmpty(seoInfo.SemanticUrl) && !string.IsNullOrEmpty(slugUrl))
                        {
                            seoInfo.SemanticUrl = slugUrl;
                        }
                        seoInfo.PageTitle ??= category.Name.SoftTruncate(70);
                    }

                    // clear category links (to save later)
                    foreach (var link in category.Links.Where(x => x.EntryId == null))
                    {
                        link.ListEntryId = category.Id;
                    }

                    categoryLinks.AddRange(category.Links);
                    category.Links = new List<CategoryLink>();

                    if (category.Level > 0)
                    {
                        if (!categoriesByHierarchyLevel.ContainsKey(category.Level))
                        {
                            categoriesByHierarchyLevel.Add(category.Level, new List<Category>());
                        }

                        categoriesByHierarchyLevel[category.Level].Add(category);
                    }
                    else
                    {
                        categories.Add(category);
                    }
                }

                // save hierarchy level 0 (root) categories
                processedCount += await SaveCategories(categories, progressInfo);
            }, _ =>
            {
                progressInfo.Description = $"{processedCount} categories have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);

            // save hierarchy level 1+ categories
            foreach (var categories in categoriesByHierarchyLevel.OrderBy(x => x.Key))
            {
                foreach (var page in categories.Value.Paginate(50))
                {
                    processedCount += await SaveCategories(page, progressInfo);

                    progressInfo.Description = $"{processedCount} categories have been imported";
                    progressCallback(progressInfo);
                }
            }

            // save category links separately after all categories are saved, to avoid DB constraint violation
            processedCount = 0;

            foreach (var page in categoryLinks.Paginate(_batchSize))
            {
                var categoryIds = page.Select(x => x.EntryId).ToList();
                var categories = await _categoryService.GetAsync(categoryIds, CategoryResponseGroup.WithLinks.ToString());

                foreach (var link in page)
                {
                    var category = categories.FirstOrDefault(x => x.Id == link.EntryId);
                    if (category?.Links.Contains(link) == false)
                    {
                        category.Links.Add(link);
                    }
                }

                if (!categories.IsNullOrEmpty())
                {
                    await _categoryService.SaveChangesAsync(categories);

                    processedCount += categories.Count;
                    progressInfo.Description = $"{processedCount} of {categoryLinks.Count} category links have been imported";
                    progressCallback(progressInfo);
                }
            }
        }

        private async Task<int> SaveCategories(IEnumerable<Category> categories, ExportImportProgressInfo progressInfo)
        {
            var itemsArray = categories.ToArray();
            await _categoryService.SaveChangesAsync(itemsArray);
            ImportImages(itemsArray.OfType<IHasImages>().ToArray(), progressInfo);

            return itemsArray.Length;
        }

        private async Task ImportPropertiesAsync(JsonTextReader reader, List<Property> propertiesWithForeignKeys, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await reader.DeserializeArrayWithPagingAsync<Property>(_jsonSerializer, _batchSize, async items =>
            {
                foreach (var property in items)
                {
                    if (property.CategoryId != null || property.CatalogId != null)
                    {
                        propertiesWithForeignKeys.Add(property.Clone() as Property);
                        //Need to reset property foreign keys to prevent FK violation during  inserting into database
                        property.CategoryId = null;
                        property.CatalogId = null;
                    }
                }
                await _propertyService.SaveChangesAsync(items);
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} properties have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ImportPropertyDictionaryItemsAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await reader.DeserializeArrayWithPagingAsync<PropertyDictionaryItem>(_jsonSerializer, _batchSize, items => _propertyDictionaryService.SaveChangesAsync(items), processedCount =>
            {
                progressInfo.Description = $"{processedCount} property dictionary items have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ImportProductsAsync(JsonTextReader reader, ExportImportOptions options, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            var associationBackupMap = new Dictionary<string, IList<ProductAssociation>>();

            await reader.DeserializeArrayWithPagingAsync<CatalogProduct>(_jsonSerializer, _batchSize, async items =>
            {
                var products = items.Select(product =>
                {
                    //Do not save associations withing product to prevent dependency conflicts in db
                    //we will save separately after product import
                    if (!product.Associations.IsNullOrEmpty())
                    {
                        associationBackupMap[product.Id] = product.Associations;
                    }

                    product.Associations = null;

                    return product;
                }).ToArray();

                await _itemService.SaveChangesAsync(products);

                if (options != null && options.HandleBinaryData)
                {
                    ImportImages(products.OfType<IHasImages>().ToArray(), progressInfo);
                }
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} products have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);

            //Import products associations separately to avoid DB constrain violation
            var totalProductsWithAssociationsCount = associationBackupMap.Count;
            for (var i = 0; i < totalProductsWithAssociationsCount; i += _batchSize)
            {
                var fakeProducts = new List<CatalogProduct>();
                foreach (var pair in associationBackupMap.Skip(i).Take(_batchSize))
                {
                    var fakeProduct = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();
                    fakeProduct.Id = pair.Key;
                    fakeProduct.Associations = pair.Value;
                    fakeProducts.Add(fakeProduct);
                }

                await _associationService.SaveChangesAsync(fakeProducts.OfType<IHasAssociations>().ToArray());
                progressInfo.Description = $"{Math.Min(totalProductsWithAssociationsCount, i + _batchSize)} of {totalProductsWithAssociationsCount} products associations imported";
                progressCallback(progressInfo);
            }
        }

        //Remove redundant references to reduce resulting JSON size
        private static void ResetRedundantReferences(object entity)
        {
            if (entity is PropertyValue propertyValue)
            {
                propertyValue.Property = null;
            }

            if (entity is ProductAssociation productAssociation)
            {
                productAssociation.AssociatedObject = null;
            }

            if (entity is Catalog catalog)
            {
                catalog.Properties = null;
                foreach (var lang in catalog.Languages)
                {
                    lang.Catalog = null;
                }
            }

            if (entity is Category category)
            {
                category.Catalog = null;
                category.Children = null;
                category.Parents = null;
                category.Outlines = null;

                if (!category.Properties.IsNullOrEmpty())
                {
                    foreach (var categoryProperty in category.Properties)
                    {
                        ResetRedundantReferences(categoryProperty);
                    }
                }
            }

            if (entity is Property property)
            {
                property.Catalog = null;
                property.Category = null;
            }

            if (entity is CatalogProduct product)
            {
                product.Catalog = null;
                product.Category = null;
                product.MainProduct = null;
                product.Outlines = null;
                product.ReferencedAssociations = null;

                if (!product.Properties.IsNullOrEmpty())
                {
                    foreach (var prop in product.Properties)
                    {
                        ResetRedundantReferences(prop);

                        foreach (var val in prop.Values)
                        {
                            ResetRedundantReferences(val);
                        }
                    }
                }

                if (!product.Associations.IsNullOrEmpty())
                {
                    foreach (var association in product.Associations)
                    {
                        ResetRedundantReferences(association);
                    }
                }

                if (!product.Variations.IsNullOrEmpty())
                {
                    foreach (var variation in product.Variations)
                    {
                        ResetRedundantReferences(variation);
                    }
                }
            }
        }

        private void LoadImages(IHasImages[] haveImagesObjects, ExportImportProgressInfo progressInfo, bool handleBinaryData)
        {
            var allImages = haveImagesObjects.SelectMany(x => x.GetFlatObjectsListWithInterface<IHasImages>())
                                             .SelectMany(x => x.Images).ToArray();
            foreach (var image in allImages)
            {
                image.Url = image.RelativeUrl;

                if (handleBinaryData && !image.HasExternalUrl)
                {
                    try
                    {
                        using (var stream = _blobStorageProvider.OpenRead(image.Url))
                        {
                            image.BinaryData = stream.ReadFully();
                        }
                    }
                    catch (Exception ex)
                    {
                        progressInfo.Errors.Add(ex.Message);
                    }
                }
            }
        }

        private void ImportImages(IHasImages[] haveImagesObjects, ExportImportProgressInfo progressInfo)
        {
            var allImages = haveImagesObjects.SelectMany(x => x.GetFlatObjectsListWithInterface<IHasImages>())
                                       .SelectMany(x => x.Images).ToArray();
            foreach (var image in allImages.Where(x => x.BinaryData != null))
            {
                try
                {
                    var url = image.Url != null && !image.Url.IsAbsoluteUrl() ? image.Url : image.RelativeUrl;
                    //do not save images with external url
                    if (!string.IsNullOrEmpty(url))
                    {
                        using (var sourceStream = new MemoryStream(image.BinaryData))
                        using (var targetStream = _blobStorageProvider.OpenWrite(image.Url))
                        {
                            sourceStream.CopyTo(targetStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    progressInfo.Errors.Add(ex.Message);
                }
            }
        }
    }
}
