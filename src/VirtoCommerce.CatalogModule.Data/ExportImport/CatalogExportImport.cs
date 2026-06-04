using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Seo.Core.Models;

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
        private readonly IProductConfigurationService _configurationService;
        private readonly IProductConfigurationSearchService _configurationSearchService;
        private readonly IMeasureService _measureService;
        private readonly IMeasureSearchService _measureSearchService;
        private readonly IPropertyGroupService _propertyGroupService;
        private readonly IPropertyGroupSearchService _propertyGroupSearchService;
        private readonly ISettingsManager _settingsManager;

        // Defaults preserve the previous hard-coded behaviour; they are overwritten from Platform
        // Settings at the start of every export/import via LoadSettingsAsync.
        private int _batchSize = 50;
        private OnImportError _importErrorPolicy = OnImportError.SkipItem;

        public CatalogExportImport(ICatalogService catalogService, ICatalogSearchService catalogSearchService, IProductSearchService productSearchService, ICategorySearchService categorySearchService, ICategoryService categoryService,
                                  IItemService itemService, IPropertyService propertyService, IPropertySearchService propertySearchService, IPropertyDictionaryItemSearchService propertyDictionarySearchService,
                                  IPropertyDictionaryItemService propertyDictionaryService, JsonSerializer jsonSerializer, IBlobStorageProvider blobStorageProvider, IAssociationService associationService,
                                  IProductConfigurationService configurationService, IProductConfigurationSearchService configurationSearchService,
                                  IMeasureService measureService, IMeasureSearchService measureSearchService, IPropertyGroupService propertyGroupService, IPropertyGroupSearchService propertyGroupSearchService,
                                  ISettingsManager settingsManager)
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
            _configurationService = configurationService;
            _configurationSearchService = configurationSearchService;
            _measureSearchService = measureSearchService;
            _measureService = measureService;
            _propertyGroupService = propertyGroupService;
            _propertyGroupSearchService = propertyGroupSearchService;
            _settingsManager = settingsManager;
        }

        // Reads the import/export tuning knobs from Platform Settings, falling back to the
        // current field values (the historical defaults) when settingsManager is unavailable.
        private async Task LoadSettingsAsync()
        {
            if (_settingsManager == null)
            {
                return;
            }

            _batchSize = await _settingsManager.GetValueAsync<int>(ModuleConstants.Settings.BackupRestore.BatchSize);

            var errorPolicy = await _settingsManager.GetValueAsync<string>(ModuleConstants.Settings.BackupRestore.ErrorPolicy);
            if (Enum.TryParse<OnImportError>(errorPolicy, ignoreCase: true, out var policy))
            {
                _importErrorPolicy = policy;
            }
        }

        public async Task DoExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await LoadSettingsAsync();

            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using var sw = new StreamWriter(outStream);
            using var writer = new JsonTextWriter(sw);
            await writer.WriteStartObjectAsync(cancellationToken);

            await ExportPropertyGroupsAsync(writer, progressInfo, progressCallback, cancellationToken);
            await ExportPropertiesAsync(writer, progressInfo, progressCallback, cancellationToken);
            await ExportPropertyDictionaryItemsAsync(writer, progressInfo, progressCallback, cancellationToken);
            await ExportCatalogsAsync(writer, progressInfo, progressCallback, cancellationToken);
            await ExportCategoriesAsync(writer, options, progressInfo, progressCallback, cancellationToken);
            await ExportProductsAsync(writer, options, progressInfo, progressCallback, cancellationToken);
            await ExportProductConfigurationsAsync(writer, progressInfo, progressCallback, cancellationToken);
            await ExportMeasuresAsync(writer, progressInfo, progressCallback, cancellationToken);

            await writer.WriteEndObjectAsync(cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }

        private async Task ExportPropertyGroupsAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Property groups exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("PropertyGroups", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _propertyGroupSearchService.SearchAsync(new PropertyGroupSearchCriteria { Skip = skip, Take = take });
                return (GenericSearchResult<PropertyGroup>)searchResult;
            }
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} property groups have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportPropertiesAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Properties exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("Properties", cancellationToken);
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
        }

        private async Task ExportPropertyDictionaryItemsAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "PropertyDictionaryItems exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("PropertyDictionaryItems", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                (GenericSearchResult<PropertyDictionaryItem>)await _propertyDictionarySearchService.SearchAsync(new PropertyDictionaryItemSearchCriteria { Skip = skip, Take = take }, clone: true)
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} property dictionary items have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportCatalogsAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Catalogs exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("Catalogs", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                (GenericSearchResult<Catalog>)await _catalogSearchService.SearchNoCloneAsync(new CatalogSearchCriteria { Skip = skip, Take = take })
            , (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} catalogs have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportCategoriesAsync(JsonTextWriter writer, ExportImportOptions options, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Categories exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("Categories", cancellationToken);
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
                progressInfo.Description = $"{processedCount} of {totalCount} categories have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportProductsAsync(JsonTextWriter writer, ExportImportOptions options, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Products exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("Products", cancellationToken);
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
                progressInfo.Description = $"{processedCount} of {totalCount} products have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportProductConfigurationsAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Product configurations exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("ProductConfigurations", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchCriteria = AbstractTypeFactory<ProductConfigurationSearchCriteria>.TryCreateInstance();
                searchCriteria.Skip = skip;
                searchCriteria.Take = take;
                var searchResult = await _configurationSearchService.SearchAsync(searchCriteria);

                foreach (var item in searchResult.Results)
                {
                    ResetRedundantReferences(item);
                }

                return (GenericSearchResult<ProductConfiguration>)searchResult;
            }, (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} product configurations have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportMeasuresAsync(JsonTextWriter writer, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Measures exporting...";
            progressCallback(progressInfo);

            await writer.WritePropertyNameAsync("Measures", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _measureSearchService.SearchAsync(new MeasureSearchCriteria { Skip = skip, Take = take, ResponseGroup = ItemResponseGroup.Full.ToString() });

                foreach (var item in searchResult.Results)
                {
                    ResetRedundantReferences(item);
                }

                return (GenericSearchResult<Measure>)searchResult;
            }, (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} measures have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private ImportStageContext BuildStage(string stage, string entityType, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback) => new()
        {
            ModuleId = ModuleConstants.ModuleId,
            Stage = stage,
            EntityType = entityType,
            ErrorPolicy = _importErrorPolicy,
            ProgressInfo = progressInfo,
            ProgressCallback = progressCallback,
        };

        public async Task DoImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await LoadSettingsAsync();

            var progressInfo = new ExportImportProgressInfo();

            var propertyGroupsWithForeignKeys = new List<PropertyGroup>();
            var propertiesWithForeignKeys = new List<Property>();

            using var streamReader = new StreamReader(inputStream);
            using var reader = new JsonTextReader(streamReader);

            var handlers = new Dictionary<string, Func<Task>>
            {
                ["PropertyGroups"] = () => ImportPropertyGroups(reader, propertyGroupsWithForeignKeys, progressInfo, progressCallback, cancellationToken),
                ["Properties"] = () => ImportPropertiesAsync(reader, propertiesWithForeignKeys, progressInfo, progressCallback, cancellationToken),
                ["PropertyDictionaryItems"] = () => ImportPropertyDictionaryItemsAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Catalogs"] = () => ImportCatalogsAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Categories"] = () => ImportCategoriesAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Products"] = () => ImportProductsAsync(reader, options, progressInfo, progressCallback, cancellationToken),
                ["ProductConfigurations"] = () => ImportProductConfigurationsAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Measures"] = () => ImportMeasuresAsync(reader, progressInfo, progressCallback, cancellationToken),
            };

            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.TokenType == JsonToken.PropertyName &&
                    handlers.TryGetValue(reader.Value.ToString(), out var handler))
                {
                    await handler();
                }
            }

            await UpdatePropertyAssociationsAsync(propertiesWithForeignKeys, progressInfo, progressCallback);
            await UpdatePropertyGroupAssociationsAsync(propertyGroupsWithForeignKeys, progressInfo, progressCallback);
        }

        private async Task UpdatePropertyAssociationsAsync(List<Property> propertiesWithForeignKeys, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            if (propertiesWithForeignKeys.Count == 0)
            {
                return;
            }

            progressInfo.Description = $"Updating {propertiesWithForeignKeys.Count} property associations…";
            progressCallback(progressInfo);

            var fkRestoreStage = BuildStage("Properties (FK-restore)", nameof(Property), progressInfo, progressCallback);
            var totalCount = propertiesWithForeignKeys.Count;
            for (var i = 0; i < totalCount; i += _batchSize)
            {
                var batch = propertiesWithForeignKeys.Skip(i).Take(_batchSize).ToList();
                await ImportStage.RunBatchAsync(fkRestoreStage, batch, items => _propertyService.SaveChangesAsync(items), x => x.Id);
                progressInfo.Description = $"{Math.Min(totalCount, i + _batchSize)} of {totalCount} property associations updated.";
                progressCallback(progressInfo);
            }
        }

        private async Task UpdatePropertyGroupAssociationsAsync(List<PropertyGroup> propertyGroupsWithForeignKeys, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            if (propertyGroupsWithForeignKeys.Count == 0)
            {
                return;
            }

            progressInfo.Description = $"Updating {propertyGroupsWithForeignKeys.Count} property group associations…";
            progressCallback(progressInfo);

            var fkRestoreStage = BuildStage("Property groups (FK-restore)", nameof(PropertyGroup), progressInfo, progressCallback);
            var totalCount = propertyGroupsWithForeignKeys.Count;
            for (var i = 0; i < totalCount; i += _batchSize)
            {
                var batch = propertyGroupsWithForeignKeys.Skip(i).Take(_batchSize).ToList();
                await ImportStage.RunBatchAsync(fkRestoreStage, batch, items => _propertyGroupService.SaveChangesAsync(items), x => x.Id);
                progressInfo.Description = $"{Math.Min(totalCount, i + _batchSize)} of {totalCount} property group associations updated.";
                progressCallback(progressInfo);
            }
        }

        private Task ImportCatalogsAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Catalogs", nameof(Catalog), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<Catalog>(_jsonSerializer, _batchSize, async catalogs =>
            {
                foreach (var catalog in catalogs)
                {
                    // Do not import property groups, they are imported separately
                    catalog.PropertyGroups = null;

                    if (catalog.SeoInfos == null || !catalog.SeoInfos.Any())
                    {
                        var defaultLanguage = catalog.Languages.First(x => x.IsDefault).LanguageCode;
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

                await ImportStage.RunBatchAsync(stage, catalogs.ToList(), items => _catalogService.SaveChangesAsync(items), x => x.Id);
            },
                processedCount =>
                {
                    progressInfo.Description = $"{processedCount} catalogs have been imported";
                    progressCallback(progressInfo);
                }, cancellationToken);
        }

        private async Task ImportCategoriesAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var processedCount = 0;
            var categoriesByHierarchyLevel = new Dictionary<int, IList<Category>>();
            var categoryLinks = new List<CategoryLink>();
            var rootStage = BuildStage("Categories Level 0", nameof(Category), progressInfo, progressCallback);

            await reader.DeserializeArrayWithPagingAsync<Category>(_jsonSerializer, _batchSize, async items =>
            {
                var categories = new List<Category>();

                foreach (var category in items)
                {
                    var slugUrl = category.Name.GenerateSlug();

                    if (category.SeoInfos.IsNullOrEmpty() && !string.IsNullOrEmpty(slugUrl))
                    {
                        var catalog = await _catalogService.GetNoCloneAsync(category.CatalogId);
                        var defaultLanguage = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                        seoInfo.LanguageCode = defaultLanguage;
                        seoInfo.SemanticUrl = slugUrl;
                        seoInfo.PageTitle = category.Name.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
                        category.SeoInfos = [seoInfo];
                    }

                    foreach (var seoInfo in category.SeoInfos)
                    {
                        if (string.IsNullOrEmpty(seoInfo.SemanticUrl) && !string.IsNullOrEmpty(slugUrl))
                        {
                            seoInfo.SemanticUrl = slugUrl;
                        }
                        seoInfo.PageTitle ??= category.Name.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
                    }

                    // clear category links (to save later)
                    foreach (var link in category.Links.Where(x => x.EntryId == null))
                    {
                        link.ListEntryId = category.Id;
                    }

                    categoryLinks.AddRange(category.Links);
                    category.Links = [];

                    if (category.Level > 0)
                    {
                        if (!categoriesByHierarchyLevel.TryGetValue(category.Level, out var levelCategories))
                        {
                            levelCategories = [];
                            categoriesByHierarchyLevel.Add(category.Level, levelCategories);
                        }

                        levelCategories.Add(category);
                    }
                    else
                    {
                        categories.Add(category);
                    }
                }

                // save hierarchy level 0 (root) categories
                processedCount += await SaveCategories(categories, rootStage, progressInfo);
            }, _ =>
            {
                progressInfo.Description = $"{processedCount} categories have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
            processedCount = await SaveCategoriesByHierarchyAsync(categoriesByHierarchyLevel, processedCount, progressInfo, progressCallback);
            await SaveCategoryLinksAsync(categoryLinks, progressInfo, progressCallback);
        }

        private async Task<int> SaveCategoriesByHierarchyAsync(Dictionary<int, IList<Category>> categoriesByHierarchyLevel, int processedCount, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            // save hierarchy level 1+ categories
            foreach (var categories in categoriesByHierarchyLevel.OrderBy(x => x.Key))
            {
                var levelStage = BuildStage($"Categories Level {categories.Key}", nameof(Category), progressInfo, progressCallback);
                foreach (var page in categories.Value.Paginate(_batchSize))
                {
                    processedCount += await SaveCategories(page, levelStage, progressInfo);

                    progressInfo.Description = $"{processedCount} categories have been imported";
                    progressCallback(progressInfo);
                }
            }
            return processedCount;
        }

        private async Task SaveCategoryLinksAsync(List<CategoryLink> categoryLinks, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            // save category links separately after all categories are saved, to avoid DB constraint violation
            var processedCount = 0;
            var linksStage = BuildStage("Category links", nameof(Category), progressInfo, progressCallback);

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
                    var saved = await ImportStage.RunBatchAsync(linksStage, categories.ToList(), items => _categoryService.SaveChangesAsync(items), x => x.Id);

                    processedCount += saved.Count;
                    progressInfo.Description = $"{processedCount} of {categoryLinks.Count} category links have been imported";
                    progressCallback(progressInfo);
                }
            }
        }

        private async Task<int> SaveCategories(IEnumerable<Category> categories, ImportStageContext stage, ExportImportProgressInfo progressInfo)
        {
            var itemsArray = categories.ToArray();
            var saved = await ImportStage.RunBatchAsync(stage, itemsArray, items => _categoryService.SaveChangesAsync(items), c => c.Id);
            if (saved.Count > 0)
            {
                // Image binaries only need to be uploaded for categories that actually persisted.
                ImportImages(saved.OfType<IHasImages>().ToArray(), progressInfo);
            }
            return saved.Count;
        }

        private Task ImportPropertyGroups(JsonTextReader reader, List<PropertyGroup> propertyGroupsWithForeignKeys, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Property groups (initial)", nameof(PropertyGroup), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<PropertyGroup>(_jsonSerializer, _batchSize, async items =>
            {
                foreach (var propertyGroup in items)
                {
                    if (propertyGroup.CatalogId != null)
                    {
                        propertyGroupsWithForeignKeys.Add(propertyGroup.CloneTyped());
                        //Need to reset property foreign keys to prevent FK violation during  inserting into database
                        propertyGroup.CatalogId = null;
                    }
                }
                await ImportStage.RunBatchAsync(stage, items.ToList(), batch => _propertyGroupService.SaveChangesAsync(batch), x => x.Id);
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} property groups have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private Task ImportPropertiesAsync(JsonTextReader reader, List<Property> propertiesWithForeignKeys, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Properties (initial)", nameof(Property), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<Property>(_jsonSerializer, _batchSize, async items =>
            {
                foreach (var property in items)
                {
                    if (property.CategoryId != null || property.CatalogId != null || property.PropertyGroupId != null)
                    {
                        propertiesWithForeignKeys.Add(property.Clone() as Property);
                        //Need to reset property foreign keys to prevent FK violation during  inserting into database
                        property.CategoryId = null;
                        property.CatalogId = null;
                    }
                }
                await ImportStage.RunBatchAsync(stage, items.ToList(), batch => _propertyService.SaveChangesAsync(batch), x => x.Id);
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} properties have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private Task ImportPropertyDictionaryItemsAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Property dictionary items", nameof(PropertyDictionaryItem), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<PropertyDictionaryItem>(_jsonSerializer, _batchSize,
                items => ImportStage.RunBatchAsync(stage, items.ToList(), batch => _propertyDictionaryService.SaveChangesAsync(batch), x => x.Id),
                processedCount =>
                {
                    progressInfo.Description = $"{processedCount} property dictionary items have been imported";
                    progressCallback(progressInfo);
                }, cancellationToken);
        }

        private async Task ImportProductsAsync(JsonTextReader reader, ExportImportOptions options, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var associationBackupMap = new Dictionary<string, IList<ProductAssociation>>();
            // De-dupe across the whole import job: manifests may list a variation both nested under
            // its parent AND as a standalone entry in manifest.Products. Without this set, the second
            // occurrence would attempt to INSERT a row whose PK already exists.
            var alreadySavedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var productsStage = BuildStage("Products", nameof(CatalogProduct), progressInfo, progressCallback);
            var variationsStage = BuildStage("Products → Variations", nameof(CatalogProduct), progressInfo, progressCallback);

            await reader.DeserializeArrayWithPagingAsync<CatalogProduct>(_jsonSerializer, _batchSize, async items =>
            {
                // Same save shape as `PUT /api/catalog/products`: parent payload never carries inline
                // Variations into SaveChangesAsync. Two flat batches per page — parents first, then
                // their variations — so a parent and its variation never share an EF tracker, but we
                // keep bulk throughput inside each batch.
                var parentsToSave = new List<CatalogProduct>();
                var variationsToSave = new List<CatalogProduct>();
                var pendingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var product in items)
                {
                    if (alreadySavedIds.Contains(product.Id) || pendingIds.Contains(product.Id))
                    {
                        continue;
                    }

                    // Capture variations off the parent; Variation : CatalogProduct, implicit upcast.
                    var capturedVariations = new List<CatalogProduct>();
                    if (!product.Variations.IsNullOrEmpty())
                    {
                        foreach (var variation in product.Variations)
                        {
                            capturedVariations.Add(variation);
                        }
                    }
                    product.Variations = null;

                    if (!product.Associations.IsNullOrEmpty())
                    {
                        associationBackupMap[product.Id] = product.Associations;
                    }
                    // Always detach associations before the parent save, even for an empty list.
                    // An empty (non-null) collection reaches ItemEntity.FromModel as a real
                    // ObservableCollection, which Patch then treats as "delete all associations"
                    // (it is not an IsNullCollection sentinel). Nulling it makes Patch skip the
                    // collection so existing DB associations survive; real associations are
                    // re-imported in the second pass below.
                    product.Associations = null;

                    parentsToSave.Add(product);
                    pendingIds.Add(product.Id);

                    foreach (var variation in capturedVariations)
                    {
                        if (alreadySavedIds.Contains(variation.Id) || pendingIds.Contains(variation.Id))
                        {
                            continue;
                        }

                        variation.MainProductId = product.Id;
                        variation.Variations = null;

                        if (!variation.Associations.IsNullOrEmpty())
                        {
                            associationBackupMap[variation.Id] = variation.Associations;
                        }
                        // See the parent block above: null even an empty list so Patch skips
                        // associations instead of clearing existing rows.
                        variation.Associations = null;

                        variationsToSave.Add(variation);
                        pendingIds.Add(variation.Id);
                    }
                }

                if (parentsToSave.Count > 0)
                {
                    var savedParents = await ImportStage.RunBatchAsync(productsStage, parentsToSave, batch => _itemService.SaveChangesAsync(batch), p => p.Id);
                    // Only mark items that actually saved as "already done". Items that failed (under
                    // SkipItem policy) must remain eligible for retry if they appear again later in the
                    // manifest (e.g. a variation that was nested under one parent and is also listed
                    // standalone). Otherwise a transient failure would silently drop the row.
                    foreach (var parent in savedParents)
                    {
                        alreadySavedIds.Add(parent.Id);
                    }
                    if (options != null && options.HandleBinaryData && savedParents.Count > 0)
                    {
                        ImportImages(savedParents.OfType<IHasImages>().ToArray(), progressInfo);
                    }
                }

                // Variations are saved AFTER parents commit — never the same tracker. If a page
                // produces more variations than ProductImportBatchSize, paginate so each
                // SaveChangesAsync call stays bounded.
                foreach (var variationBatch in variationsToSave.Paginate(_batchSize))
                {
                    var savedVariations = await ImportStage.RunBatchAsync(variationsStage, variationBatch.ToList(), batch => _itemService.SaveChangesAsync(batch), p => p.Id);
                    foreach (var variation in savedVariations)
                    {
                        alreadySavedIds.Add(variation.Id);
                    }
                    if (options != null && options.HandleBinaryData && savedVariations.Count > 0)
                    {
                        ImportImages(savedVariations.OfType<IHasImages>().ToArray(), progressInfo);
                    }
                }
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} products have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);

            //Import products associations separately to avoid DB constrain violation
            var totalProductsWithAssociationsCount = associationBackupMap.Count;
            var associationsStage = BuildStage("Products → Associations", nameof(CatalogProduct), progressInfo, progressCallback);
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

                await ImportStage.RunBatchAsync(associationsStage, fakeProducts, batch => _associationService.SaveChangesAsync(batch.OfType<IHasAssociations>().ToArray()), p => p.Id);
                progressInfo.Description = $"{Math.Min(totalProductsWithAssociationsCount, i + _batchSize)} of {totalProductsWithAssociationsCount} products associations imported";
                progressCallback(progressInfo);
            }
        }

        private Task ImportProductConfigurationsAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Product configurations", nameof(ProductConfiguration), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<ProductConfiguration>(_jsonSerializer, _batchSize, async configurations =>
            {
                foreach (var configuration in configurations)
                {
                    // Configuration with any sections can be active
                    if (configuration.Sections.IsNullOrEmpty())
                    {
                        configuration.IsActive = false;
                    }

                    await ImportStage.RunBatchAsync(stage, new[] { configuration }, batch => _configurationService.SaveChangesAsync(batch), x => x.Id);
                }
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} product configurations have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private Task ImportMeasuresAsync(JsonTextReader reader, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var stage = BuildStage("Measures", nameof(Measure), progressInfo, progressCallback);

            return reader.DeserializeArrayWithPagingAsync<Measure>(_jsonSerializer, _batchSize, async measures =>
            {
                await ImportStage.RunBatchAsync(stage, measures.ToList(), batch => _measureService.SaveChangesAsync(batch), x => x.Id);
            }, processedCount =>
            {
                progressInfo.Description = $"{processedCount} measures have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
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
                catalog.PropertyGroups = null;
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

            if (entity is ProductConfiguration configuration)
            {
                foreach (var section in configuration.Sections)
                {
                    foreach (var option in section.Options)
                    {
                        option.Product = null;
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
                        using var stream = _blobStorageProvider.OpenRead(image.Url);
                        image.BinaryData = stream.ReadFully();
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
                        using var sourceStream = new MemoryStream(image.BinaryData);
                        using var targetStream = _blobStorageProvider.OpenWrite(image.Url);
                        sourceStream.CopyTo(targetStream);
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
