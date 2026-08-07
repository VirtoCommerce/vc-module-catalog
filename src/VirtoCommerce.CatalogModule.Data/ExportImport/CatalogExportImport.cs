using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private sealed class ProductImportContext
        {
            public ExportImportOptions Options { get; init; }
            public CatalogImportPackage Package { get; init; }
            public Dictionary<string, IList<ProductAssociation>> AssociationBackupMap { get; init; }
            public HashSet<string> AlreadySavedIds { get; init; }
            public ImportStageContext ProductsStage { get; init; }
            public ImportStageContext VariationsStage { get; init; }
            public ExportImportProgressInfo ProgressInfo { get; init; }
            public CancellationToken CancellationToken { get; init; }
        }

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

            using var package = CatalogExportPackage.Create(outStream, options?.HandleBinaryData == true);
            using (var streamWriter = new StreamWriter(package.CatalogStream, new UTF8Encoding(false), 1024, leaveOpen: true))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                await writer.WriteStartObjectAsync(cancellationToken);

                await ExportPropertyGroupsAsync(writer, progressInfo, progressCallback, cancellationToken);
                await ExportPropertiesAsync(writer, progressInfo, progressCallback, cancellationToken);
                await ExportPropertyDictionaryItemsAsync(writer, progressInfo, progressCallback, cancellationToken);
                await ExportCatalogsAsync(writer, progressInfo, progressCallback, cancellationToken);
                await ExportCategoriesAsync(writer, options, package, progressInfo, progressCallback, cancellationToken);
                await ExportProductsAsync(writer, options, package, progressInfo, progressCallback, cancellationToken);
                await ExportProductConfigurationsAsync(writer, progressInfo, progressCallback, cancellationToken);
                await ExportMeasuresAsync(writer, progressInfo, progressCallback, cancellationToken);

                await writer.WriteEndObjectAsync(cancellationToken);
                await writer.FlushAsync(cancellationToken);
            }

            await package.CompleteAsync(cancellationToken);
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

        private async Task ExportCategoriesAsync(JsonTextWriter writer, ExportImportOptions options, CatalogExportPackage package, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Categories exporting...";
            progressCallback(progressInfo);

            var isCountQuery = true;
            await writer.WritePropertyNameAsync("Categories", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _categorySearchService.SearchAsync(new CategorySearchCriteria { Skip = skip, Take = take });
                if (isCountQuery)
                {
                    isCountQuery = false;
                    return (GenericSearchResult<Category>)searchResult;
                }

                foreach (var item in searchResult.Results)
                {
                    ResetRedundantReferences(item);
                }

                await ExportBinaryDataAsync(searchResult.Results, options?.HandleBinaryData == true, package, progressInfo, cancellationToken);
                return (GenericSearchResult<Category>)searchResult;
            }, (processedCount, totalCount) =>
            {
                progressInfo.Description = $"{processedCount} of {totalCount} categories have been exported";
                progressCallback(progressInfo);
            }, cancellationToken);
        }

        private async Task ExportProductsAsync(JsonTextWriter writer, ExportImportOptions options, CatalogExportPackage package, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            progressInfo.Description = "Products exporting...";
            progressCallback(progressInfo);

            var isCountQuery = true;
            await writer.WritePropertyNameAsync("Products", cancellationToken);
            await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
            {
                var searchResult = await _productSearchService.SearchAsync(new ProductSearchCriteria { Skip = skip, Take = take, ResponseGroup = ItemResponseGroup.Full.ToString() });
                if (isCountQuery)
                {
                    isCountQuery = false;
                    return (GenericSearchResult<CatalogProduct>)searchResult;
                }

                foreach (var item in searchResult.Results)
                {
                    ResetRedundantReferences(item);
                }

                await ExportBinaryDataAsync(searchResult.Results, options?.HandleBinaryData == true, package, progressInfo, cancellationToken);
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

            using var package = await CatalogImportPackage.OpenAsync(inputStream, cancellationToken);
            using var streamReader = new StreamReader(package.CatalogStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
            using var reader = new JsonTextReader(streamReader);

            var handlers = new Dictionary<string, Func<Task>>
            {
                ["PropertyGroups"] = () => ImportPropertyGroups(reader, propertyGroupsWithForeignKeys, progressInfo, progressCallback, cancellationToken),
                ["Properties"] = () => ImportPropertiesAsync(reader, propertiesWithForeignKeys, progressInfo, progressCallback, cancellationToken),
                ["PropertyDictionaryItems"] = () => ImportPropertyDictionaryItemsAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Catalogs"] = () => ImportCatalogsAsync(reader, progressInfo, progressCallback, cancellationToken),
                ["Categories"] = () => ImportCategoriesAsync(reader, options, package, progressInfo, progressCallback, cancellationToken),
                ["Products"] = () => ImportProductsAsync(reader, options, package, progressInfo, progressCallback, cancellationToken),
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

        private async Task ImportCategoriesAsync(JsonTextReader reader, ExportImportOptions options, CatalogImportPackage package, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var processedCount = 0;
            var categoriesByHierarchyLevel = new Dictionary<int, IList<Category>>();
            var categoryLinks = new List<CategoryLink>();
            var rootStage = BuildStage("Categories Level 0", nameof(Category), progressInfo, progressCallback);

            await reader.DeserializeArrayWithPagingAsync<Category>(_jsonSerializer, _batchSize, async items =>
            {
                if (options?.HandleBinaryData != true)
                {
                    ClearBinaryData(items);
                }

                var categories = await PrepareCategoriesAsync(items, categoriesByHierarchyLevel, categoryLinks);

                // save hierarchy level 0 (root) categories
                processedCount += await SaveCategories(categories, options, package, rootStage, progressInfo, cancellationToken);
            }, _ =>
            {
                progressInfo.Description = $"{processedCount} categories have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);
            processedCount = await SaveCategoriesByHierarchyAsync(categoriesByHierarchyLevel, options, package, processedCount, progressInfo, progressCallback, cancellationToken);
            await SaveCategoryLinksAsync(categoryLinks, progressInfo, progressCallback);
        }

        private async Task<List<Category>> PrepareCategoriesAsync(
            IEnumerable<Category> categories,
            IDictionary<int, IList<Category>> categoriesByHierarchyLevel,
            ICollection<CategoryLink> categoryLinks)
        {
            var rootCategories = new List<Category>();

            foreach (var category in categories)
            {
                await PrepareCategorySeoAsync(category);
                DetachCategoryLinks(category, categoryLinks);
                AddCategoryByHierarchyLevel(category, rootCategories, categoriesByHierarchyLevel);
            }

            return rootCategories;
        }

        private async Task PrepareCategorySeoAsync(Category category)
        {
            var slugUrl = category.Name.GenerateSlug();

            if (category.SeoInfos.IsNullOrEmpty() && !string.IsNullOrEmpty(slugUrl))
            {
                var catalog = await _catalogService.GetNoCloneAsync(category.CatalogId);
                var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                seoInfo.LanguageCode = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                seoInfo.SemanticUrl = slugUrl;
                seoInfo.PageTitle = category.Name.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
                category.SeoInfos = [seoInfo];
            }

            foreach (var seoInfo in category.SeoInfos)
            {
                SetCategorySeoDefaults(seoInfo, category.Name, slugUrl);
            }
        }

        private static void SetCategorySeoDefaults(SeoInfo seoInfo, string categoryName, string slugUrl)
        {
            if (string.IsNullOrEmpty(seoInfo.SemanticUrl) && !string.IsNullOrEmpty(slugUrl))
            {
                seoInfo.SemanticUrl = slugUrl;
            }

            seoInfo.PageTitle ??= categoryName.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
        }

        private static void DetachCategoryLinks(Category category, ICollection<CategoryLink> categoryLinks)
        {
            foreach (var link in category.Links.Where(x => x.EntryId == null))
            {
                link.ListEntryId = category.Id;
            }

            categoryLinks.AddRange(category.Links);
            category.Links = [];
        }

        private static void AddCategoryByHierarchyLevel(
            Category category,
            List<Category> rootCategories,
            IDictionary<int, IList<Category>> categoriesByHierarchyLevel)
        {
            if (category.Level <= 0)
            {
                rootCategories.Add(category);
                return;
            }

            if (!categoriesByHierarchyLevel.TryGetValue(category.Level, out var levelCategories))
            {
                levelCategories = [];
                categoriesByHierarchyLevel.Add(category.Level, levelCategories);
            }

            levelCategories.Add(category);
        }

        private async Task<int> SaveCategoriesByHierarchyAsync(
            Dictionary<int, IList<Category>> categoriesByHierarchyLevel,
            ExportImportOptions options,
            CatalogImportPackage package,
            int processedCount,
            ExportImportProgressInfo progressInfo,
            Action<ExportImportProgressInfo> progressCallback,
            CancellationToken cancellationToken)
        {
            // save hierarchy level 1+ categories
            foreach (var categories in categoriesByHierarchyLevel.OrderBy(x => x.Key))
            {
                var levelStage = BuildStage($"Categories Level {categories.Key}", nameof(Category), progressInfo, progressCallback);
                foreach (var page in categories.Value.Paginate(_batchSize))
                {
                    processedCount += await SaveCategories(page, options, package, levelStage, progressInfo, cancellationToken);

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

        private async Task<int> SaveCategories(IEnumerable<Category> categories, ExportImportOptions options, CatalogImportPackage package, ImportStageContext stage, ExportImportProgressInfo progressInfo, CancellationToken cancellationToken)
        {
            var itemsArray = categories.ToArray();
            var saved = await ImportStage.RunBatchAsync(stage, itemsArray, items => _categoryService.SaveChangesAsync(items), c => c.Id);
            if (options?.HandleBinaryData == true && saved.Count > 0)
            {
                // Binaries only need to be uploaded for categories that actually persisted.
                await ImportBinaryDataAsync(saved, package, progressInfo, cancellationToken);
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

        private async Task ImportProductsAsync(JsonTextReader reader, ExportImportOptions options, CatalogImportPackage package, ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            var associationBackupMap = new Dictionary<string, IList<ProductAssociation>>();
            var alreadySavedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var productsStage = BuildStage("Products", nameof(CatalogProduct), progressInfo, progressCallback);
            var variationsStage = BuildStage("Products → Variations", nameof(CatalogProduct), progressInfo, progressCallback);
            var context = new ProductImportContext
            {
                Options = options,
                Package = package,
                AssociationBackupMap = associationBackupMap,
                AlreadySavedIds = alreadySavedIds,
                ProductsStage = productsStage,
                VariationsStage = variationsStage,
                ProgressInfo = progressInfo,
                CancellationToken = cancellationToken,
            };

            await reader.DeserializeArrayWithPagingAsync<CatalogProduct>(
                _jsonSerializer,
                _batchSize,
                items => ImportProductPageAsync(items, context),
                processedCount =>
            {
                progressInfo.Description = $"{processedCount} products have been imported";
                progressCallback(progressInfo);
            }, cancellationToken);

            await ImportProductAssociationsAsync(associationBackupMap, progressInfo, progressCallback);
        }

        private async Task ImportProductPageAsync(
            IEnumerable<CatalogProduct> products,
            ProductImportContext context)
        {
            if (context.Options?.HandleBinaryData != true)
            {
                ClearBinaryData(products);
            }

            var (parentsToSave, variationsToSave) = PrepareProducts(
                products,
                context.AssociationBackupMap,
                context.AlreadySavedIds);

            await SaveProductBatchAsync(parentsToSave, context.ProductsStage, context);
            await SaveVariationsAsync(variationsToSave, context);
        }

        private static (List<CatalogProduct> Parents, List<CatalogProduct> Variations) PrepareProducts(
            IEnumerable<CatalogProduct> products,
            Dictionary<string, IList<ProductAssociation>> associationBackupMap,
            HashSet<string> alreadySavedIds)
        {
            var parentsToSave = new List<CatalogProduct>();
            var variationsToSave = new List<CatalogProduct>();
            var pendingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var product in products)
            {
                if (!TryReserveProduct(product.Id, alreadySavedIds, pendingIds))
                {
                    continue;
                }

                var variations = DetachVariations(product);
                DetachAssociations(product, associationBackupMap);
                parentsToSave.Add(product);

                foreach (var variation in variations)
                {
                    if (!TryReserveProduct(variation.Id, alreadySavedIds, pendingIds))
                    {
                        continue;
                    }

                    variation.MainProductId = product.Id;
                    variation.Variations = null;
                    DetachAssociations(variation, associationBackupMap);
                    variationsToSave.Add(variation);
                }
            }

            return (parentsToSave, variationsToSave);
        }

        private static bool TryReserveProduct(string productId, HashSet<string> alreadySavedIds, HashSet<string> pendingIds)
        {
            return !alreadySavedIds.Contains(productId) && pendingIds.Add(productId);
        }

        private static List<CatalogProduct> DetachVariations(CatalogProduct product)
        {
            var variations = product.Variations?.Cast<CatalogProduct>().ToList() ?? [];
            product.Variations = null;
            return variations;
        }

        private static void DetachAssociations(
            CatalogProduct product,
            Dictionary<string, IList<ProductAssociation>> associationBackupMap)
        {
            if (!product.Associations.IsNullOrEmpty())
            {
                associationBackupMap[product.Id] = product.Associations;
            }

            // A null collection makes Patch preserve existing associations until the second import pass.
            product.Associations = null;
        }

        private async Task SaveVariationsAsync(
            IEnumerable<CatalogProduct> variations,
            ProductImportContext context)
        {
            foreach (var batch in variations.Paginate(_batchSize))
            {
                await SaveProductBatchAsync(batch.ToList(), context.VariationsStage, context);
            }
        }

        private async Task SaveProductBatchAsync(
            List<CatalogProduct> products,
            ImportStageContext stage,
            ProductImportContext context)
        {
            if (products.Count == 0)
            {
                return;
            }

            var savedProducts = await ImportStage.RunBatchAsync(
                stage,
                products,
                batch => _itemService.SaveChangesAsync(batch),
                product => product.Id);

            foreach (var product in savedProducts)
            {
                context.AlreadySavedIds.Add(product.Id);
            }

            if (context.Options?.HandleBinaryData == true && savedProducts.Count > 0)
            {
                await ImportBinaryDataAsync(savedProducts, context.Package, context.ProgressInfo, context.CancellationToken);
            }
        }

        private async Task ImportProductAssociationsAsync(
            Dictionary<string, IList<ProductAssociation>> associationBackupMap,
            ExportImportProgressInfo progressInfo,
            Action<ExportImportProgressInfo> progressCallback)
        {
            var totalProductsWithAssociationsCount = associationBackupMap.Count;
            var associationsStage = BuildStage("Products → Associations", nameof(CatalogProduct), progressInfo, progressCallback);

            for (var i = 0; i < totalProductsWithAssociationsCount; i += _batchSize)
            {
                var fakeProducts = CreateProductsForAssociationImport(associationBackupMap.Skip(i).Take(_batchSize));
                await ImportStage.RunBatchAsync(
                    associationsStage,
                    fakeProducts,
                    batch => _associationService.SaveChangesAsync(batch.OfType<IHasAssociations>().ToArray()),
                    product => product.Id);

                progressInfo.Description = $"{Math.Min(totalProductsWithAssociationsCount, i + _batchSize)} of {totalProductsWithAssociationsCount} products associations imported";
                progressCallback(progressInfo);
            }
        }

        private static List<CatalogProduct> CreateProductsForAssociationImport(
            IEnumerable<KeyValuePair<string, IList<ProductAssociation>>> associations)
        {
            var products = new List<CatalogProduct>();

            foreach (var pair in associations)
            {
                var product = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();
                product.Id = pair.Key;
                product.Associations = pair.Value;
                products.Add(product);
            }

            return products;
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

        private async Task ExportBinaryDataAsync<T>(IEnumerable<T> entities, bool handleBinaryData, CatalogExportPackage package, ExportImportProgressInfo progressInfo, CancellationToken cancellationToken)
        {
            foreach (var image in GetImages(entities))
            {
                var sourceUrl = GetRelativeUrl(image);
                if (!string.IsNullOrEmpty(sourceUrl))
                {
                    image.Url = sourceUrl;
                }

                image.BinaryData = null;
                image.BinaryDataReference = null;

                if (handleBinaryData && !string.IsNullOrEmpty(sourceUrl))
                {
                    await ExportBinaryDataAsync(image, sourceUrl, package, progressInfo, cancellationToken);
                }
            }

            foreach (var asset in GetAssets(entities))
            {
                var sourceUrl = GetRelativeUrl(asset);
                asset.BinaryData = null;
                asset.BinaryDataReference = null;

                if (handleBinaryData && !string.IsNullOrEmpty(sourceUrl))
                {
                    asset.Url = sourceUrl;
                    await ExportBinaryDataAsync(asset, sourceUrl, package, progressInfo, cancellationToken);
                }
            }
        }

        private async Task ExportBinaryDataAsync(AssetBase asset, string sourceUrl, CatalogExportPackage package, ExportImportProgressInfo progressInfo, CancellationToken cancellationToken)
        {
            try
            {
                asset.BinaryDataReference = await package.WriteBinaryDataAsync(
                    sourceUrl,
                    () => _blobStorageProvider.OpenReadAsync(sourceUrl),
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                progressInfo.Errors ??= [];
                progressInfo.Errors.Add(ex.Message);
            }
        }

        private async Task ImportBinaryDataAsync<T>(IEnumerable<T> entities, CatalogImportPackage package, ExportImportProgressInfo progressInfo, CancellationToken cancellationToken)
        {
            foreach (var image in GetImages(entities))
            {
                await ImportBinaryDataAsync(image, image.BinaryData, package, progressInfo, cancellationToken);
            }

            foreach (var asset in GetAssets(entities))
            {
                await ImportBinaryDataAsync(asset, asset.BinaryData, package, progressInfo, cancellationToken);
            }
        }

        private async Task ImportBinaryDataAsync(AssetBase asset, byte[] inlineBinaryData, CatalogImportPackage package, ExportImportProgressInfo progressInfo, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (inlineBinaryData == null && string.IsNullOrEmpty(asset.BinaryDataReference))
                {
                    return;
                }

                var url = GetRelativeUrl(asset);
                if (string.IsNullOrEmpty(url))
                {
                    return;
                }

                if (inlineBinaryData != null)
                {
                    // Legacy JSON packages already materialize the base64 value during deserialization.
                    await using (var targetStream = await _blobStorageProvider.OpenWriteAsync(url))
                    {
                        await targetStream.WriteAsync(inlineBinaryData, cancellationToken);
                    }
                }
                else
                {
                    if (!CatalogPackageFormat.IsValidBinaryDataReference(asset.BinaryDataReference))
                    {
                        throw new InvalidDataException($"The binary data reference '{asset.BinaryDataReference}' is invalid.");
                    }

                    var expectedReference = CatalogPackageFormat.CreateBinaryDataReference(url);
                    if (!string.Equals(asset.BinaryDataReference, expectedReference, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException($"Binary data reference '{asset.BinaryDataReference}' does not match destination URL '{url}'.");
                    }

                    if (package.IsBinaryDataImported(asset.BinaryDataReference, url))
                    {
                        return;
                    }

                    await using (var sourceStream = package.OpenBinaryData(asset.BinaryDataReference))
                    await using (var targetStream = await _blobStorageProvider.OpenWriteAsync(url))
                    {
                        await sourceStream.CopyToAsync(targetStream, cancellationToken);
                    }

                    package.MarkBinaryDataImported(asset.BinaryDataReference, url);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                progressInfo.Errors ??= [];
                progressInfo.Errors.Add(ex.Message);
            }
            finally
            {
                ClearBinaryData(asset);
            }
        }

        private static IEnumerable<Image> GetImages<T>(IEnumerable<T> entities)
        {
            return new { entities }
                .GetFlatObjectsListWithInterface<IHasImages>()
                .Where(x => !x.Images.IsNullOrEmpty())
                .SelectMany(x => x.Images);
        }

        private static IEnumerable<Asset> GetAssets<T>(IEnumerable<T> entities)
        {
            return new { entities }
                .GetFlatObjectsListWithInterface<IHasAssets>()
                .Where(x => !x.Assets.IsNullOrEmpty())
                .SelectMany(x => x.Assets);
        }

        private static string GetRelativeUrl(AssetBase asset)
        {
            if (!string.IsNullOrWhiteSpace(asset.RelativeUrl))
            {
                return IsRelativeBlobUrl(asset.RelativeUrl) ? asset.RelativeUrl : null;
            }

            return IsRelativeBlobUrl(asset.Url) ? asset.Url : null;
        }

        private static bool IsRelativeBlobUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url)
                && !url.StartsWith("//", StringComparison.Ordinal)
                && !Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private static void ClearBinaryData<T>(IEnumerable<T> entities)
        {
            foreach (var image in GetImages(entities))
            {
                ClearBinaryData(image);
            }

            foreach (var asset in GetAssets(entities))
            {
                ClearBinaryData(asset);
            }
        }

        private static void ClearBinaryData(AssetBase asset)
        {
            asset.BinaryDataReference = null;

            if (asset is Image image)
            {
                image.BinaryData = null;
            }
            else if (asset is Asset file)
            {
                file.BinaryData = null;
            }
        }
    }
}
