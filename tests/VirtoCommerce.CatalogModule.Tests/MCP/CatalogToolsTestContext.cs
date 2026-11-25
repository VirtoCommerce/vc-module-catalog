using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.MCP;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.MCP.Testing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.JsonConverters;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;
using CatalogSettings = VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings;
using SearchSettings = VirtoCommerce.SearchModule.Core.ModuleConstants.Settings;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

// The catalog manager tools over the test data, with the real catalog authorization handler and the real product setup:
// only the services that reach the database and the search index are doubles.
public static class CatalogToolsTestContext
{
    public const string PublicOrigin = "https://admin.example.com";
    public const string CatalogScope = "SelectedCatalogScope";

    private static readonly Lock _jsonLock = new();
    private static IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;

    public static readonly string[] CatalogReadPermissions = ["catalog:read", "catalog:categories:read", "catalog:products:read"];

    // A: the catalog permissions granted globally.
    public static ClaimsPrincipal GlobalManager()
    {
        return McpTestUser.Create("global-manager").WithPermissions(["catalog:access", "store:read", .. CatalogReadPermissions]);
    }

    // B and C: every catalog permission that can be limited, limited to one catalog, and store:read for every store.
    public static ClaimsPrincipal ManagerOf(string catalogId)
    {
        var user = McpTestUser.Create($"manager-of-{catalogId}").WithPermissions("catalog:access", "store:read");

        foreach (var permission in CatalogReadPermissions)
        {
            user.WithScopedPermission(permission, CatalogScope, catalogId);
        }

        return user;
    }

    // configure: what a test adds, such as the part of another module.
    public static McpToolHarness CreateHarness(CatalogToolsTestData data, Action<IServiceCollection> configure = null)
    {
        return McpToolHarness.Create<McpAssemblyMarker>(services =>
        {
            AddCatalogServices(services, data);
            services.AddCatalogMcpServices();
            services.AddSingleton<IMcpPageSizeResolver, FixedPageSizeResolver>();
            services.AddSingleton<IMcpPublicOriginResolver>(new McpPublicOriginResolver(Options.Create(new McpOptions { PublicOrigin = PublicOrigin })));
            configure?.Invoke(services);
        });
    }

    public static Task<McpToolCallResult> CallAsync(McpToolHarness harness, ClaimsPrincipal user, string toolName, object arguments = null)
    {
        return harness.CallAsync(user, toolName, arguments, cancellationToken: TestContext.Current.CancellationToken);
    }

    // Catalog's product setup as Module.Initialize registers it, over the test data.
    public static void AddCatalogServices(IServiceCollection services, CatalogToolsTestData data, bool eventIndexing = true, bool scheduledIndexing = true)
    {
        services.AddLogging();
        services.AddTransient<IProductSetupService, ProductSetupService>();
        services.AddTransient<IProductSetupFindings, ProductSetupFindings>();
        services.AddTransient<ProductIndexingTexts>();
        services.AddProductSetupPart<CatalogProductSetupPart>();

        services.AddAuthorizationCore();
        services.AddSingleton(JsonOptions);
        services.AddTransient<IAuthorizationHandler, CatalogAuthorizationHandler>();

        services.AddSingleton(new InMemoryProductIndex(data));
        services.AddSingleton<ISearchProvider>(x => x.GetRequiredService<InMemoryProductIndex>());
        services.AddSingleton(CreateStoreService(data));
        services.AddSingleton(CreateStoreSearchService(data));
        services.AddSingleton(CreateCatalogService(data));
        services.AddSingleton(CreateCategoryService(data));
        services.AddSingleton(CreateCategorySearchService(data));
        services.AddSingleton(CreateItemService(data));
        services.AddSingleton(CreateProductSearchService(data));
        services.AddSingleton(CreateLinkSearchService(data));
        services.AddSingleton(Mock.Of<IPropertyDictionaryItemSearchService>(x =>
            x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()) == Task.FromResult(new PropertyDictionaryItemSearchResult())));
        services.AddSingleton<Func<ICatalogRepository>>(() => CreateRepository(data));
        services.AddSingleton(CreateSettingsManager(eventIndexing, scheduledIndexing));
    }

    // The platform's JSON settings for permission scopes, which the catalog handler reads the scoped claims with: the
    // PolymorphJsonConverter with the scope type as discriminator, and Catalog's scope registered.
    public static IOptions<MvcNewtonsoftJsonOptions> JsonOptions
    {
        get
        {
            lock (_jsonLock)
            {
                if (_jsonOptions == null)
                {
                    AbstractTypeFactory<PermissionScope>.RegisterType<SelectedCatalogScope>();
                    PolymorphJsonConverter.RegisterTypeForDiscriminator(typeof(PermissionScope), nameof(PermissionScope.Type));

                    var options = new MvcNewtonsoftJsonOptions();
                    options.SerializerSettings.ContractResolver = new PolymorphJsonContractResolver();
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new PolymorphJsonConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    _jsonOptions = Options.Create(options);
                }

                return _jsonOptions;
            }
        }
    }

    private static IStoreService CreateStoreService(CatalogToolsTestData data)
    {
        var storeService = new Mock<IStoreService>();
        storeService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => data.Stores.Where(x => ids.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).Select(x => x.CloneTyped()).ToList());

        return storeService.Object;
    }

    private static IStoreSearchService CreateStoreSearchService(CatalogToolsTestData data)
    {
        var searchService = new Mock<IStoreSearchService>();
        searchService
            .Setup(x => x.SearchAsync(It.IsAny<StoreSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((StoreSearchCriteria criteria, bool _) =>
            {
                var found = data.Stores
                    .Where(x => criteria.ObjectIds.IsNullOrEmpty() || criteria.ObjectIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new StoreSearchResult { TotalCount = found.Count, Results = found.Skip(criteria.Skip).Take(criteria.Take).Select(x => x.CloneTyped()).ToList() };
            });

        return searchService.Object;
    }

    private static ICatalogService CreateCatalogService(CatalogToolsTestData data)
    {
        var catalogService = new Mock<ICatalogService>();
        catalogService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => data.Catalogs.Where(x => ids.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).Select(x => x.CloneTyped()).ToList());

        return catalogService.Object;
    }

    private static ICategoryService CreateCategoryService(CatalogToolsTestData data)
    {
        var categoryService = new Mock<ICategoryService>();
        categoryService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => data.Categories.Where(x => ids.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).Select(x => LoadCategory(data, x)).ToList());

        return categoryService.Object;
    }

    private static ICategorySearchService CreateCategorySearchService(CatalogToolsTestData data)
    {
        var searchService = new Mock<ICategorySearchService>();
        searchService
            .Setup(x => x.SearchAsync(It.IsAny<CategorySearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((CategorySearchCriteria criteria, bool _) =>
            {
                var found = data.Categories
                    .Where(x => criteria.CatalogIds.IsNullOrEmpty() || criteria.CatalogIds.Contains(x.CatalogId))
                    .Where(x => criteria.SearchOnlyInRoot ? x.ParentId == null : criteria.CategoryId == null || x.ParentId == criteria.CategoryId)
                    .ToList();

                return new CategorySearchResult { TotalCount = found.Count, Results = found.Skip(criteria.Skip).Take(criteria.Take).Select(x => LoadCategory(data, x)).ToList() };
            });

        return searchService.Object;
    }

    private static IItemService CreateItemService(CatalogToolsTestData data)
    {
        var itemService = new Mock<IItemService>();
        itemService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => data.Products.Where(x => ids.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).Select(x => LoadProduct(data, x)).ToList());

        return itemService.Object;
    }

    // The database search of products: it fails a search that would read a catalog's products in full, by a criteria that
    // names no SKU, id, main product or category.
    private static IProductSearchService CreateProductSearchService(CatalogToolsTestData data)
    {
        var searchService = new Mock<IProductSearchService>();
        searchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((ProductSearchCriteria criteria, bool _) =>
            {
                if (criteria.Skus.IsNullOrEmpty() && criteria.ObjectIds.IsNullOrEmpty() && string.IsNullOrEmpty(criteria.MainProductId) && criteria.CategoryIds.IsNullOrEmpty())
                {
                    throw new InvalidOperationException("A product search would read a catalog's products in full.");
                }

                var categoryIds = criteria.CategoryIds.IsNullOrEmpty()
                    ? null
                    : data.Categories.Where(x => criteria.CategoryIds.Contains(x.Id) || (criteria.SearchInChildren && IsUnder(data, x, criteria.CategoryIds))).Select(x => x.Id).ToHashSet();
                var found = data.Products
                    .Where(x => criteria.CatalogIds.IsNullOrEmpty() || criteria.CatalogIds.Contains(x.CatalogId))
                    .Where(x => criteria.Skus.IsNullOrEmpty() || criteria.Skus.Contains(x.Code))
                    .Where(x => criteria.ObjectIds.IsNullOrEmpty() || criteria.ObjectIds.Contains(x.Id))
                    .Where(x => categoryIds == null || categoryIds.Contains(x.CategoryId))
                    .Where(x => string.IsNullOrEmpty(criteria.MainProductId) ? criteria.SearchInVariations || x.MainProductId == null : x.MainProductId == criteria.MainProductId)
                    .ToList();

                return new ProductSearchResult { TotalCount = found.Count, Results = found.Skip(criteria.Skip).Take(criteria.Take).Select(x => LoadProduct(data, x)).ToList() };
            });

        return searchService.Object;
    }

    private static ILinkSearchService CreateLinkSearchService(CatalogToolsTestData data)
    {
        var searchService = new Mock<ILinkSearchService>();
        searchService
            .Setup(x => x.SearchAsync(It.IsAny<LinkSearchCriteria>()))
            .ReturnsAsync((LinkSearchCriteria criteria) =>
            {
                // As LinkSearchService: category ids match links into those categories; catalog ids only links into a root.
                var found = data.CategoryLinks
                    .Where(x => x.ListEntryType.EqualsIgnoreCase(criteria.ObjectType))
                    .Where(x => !criteria.CategoryIds.IsNullOrEmpty()
                        ? criteria.CategoryIds.Contains(x.CategoryId)
                        : criteria.CatalogIds.IsNullOrEmpty() || (criteria.CatalogIds.Contains(x.CatalogId) && x.CategoryId == null))
                    .ToList();

                return new LinkSearchResult { TotalCount = found.Count, Results = found.Skip(criteria.Skip).Take(criteria.Take).ToList() };
            });

        return searchService.Object;
    }

    // The repository queries of the product setup: links by target and products by main product.
    private static ICatalogRepository CreateRepository(CatalogToolsTestData data)
    {
        var categoryEntities = data.Categories.ToDictionary(x => x.Id, x => new CategoryEntity { Id = x.Id, CatalogId = x.CatalogId, ParentCategoryId = x.ParentId });
        var categoryLinks = data.CategoryLinks
            .Where(x => x.ListEntryType == nameof(Category))
            .Select(x => new CategoryRelationEntity
            {
                SourceCategoryId = x.ListEntryId,
                SourceCategory = categoryEntities[x.ListEntryId],
                TargetCatalogId = x.CatalogId ?? data.GetCategory(x.CategoryId).CatalogId,
                TargetCategoryId = x.CategoryId,
                TargetCategory = x.CategoryId == null ? null : categoryEntities[x.CategoryId],
            })
            .ToList();
        var items = data.Products.Select(x => new ItemEntity { Id = x.Id, CatalogId = x.CatalogId, ParentId = x.MainProductId }).ToList();

        var repository = new Mock<ICatalogRepository>();
        repository.Setup(x => x.CategoryLinks).Returns(categoryLinks.BuildMock());
        repository.Setup(x => x.CategoryItemRelations).Returns(new List<CategoryItemRelationEntity>().BuildMock());
        repository.Setup(x => x.Items).Returns(items.BuildMock());
        repository
            .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync((IList<string> ids) => data.Categories.Where(x => IsUnder(data, x, ids)).Select(x => x.Id).ToList());

        return repository.Object;
    }

    private static ISettingsManager CreateSettingsManager(bool eventIndexing, bool scheduledIndexing)
    {
        var settingsManager = new Mock<ISettingsManager>();
        settingsManager
            .Setup(x => x.GetObjectSettingAsync(CatalogSettings.General.EventBasedIndexation.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry(CatalogSettings.General.EventBasedIndexation) { Value = eventIndexing });
        settingsManager
            .Setup(x => x.GetObjectSettingAsync(SearchSettings.IndexingJobs.Enable.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry(SearchSettings.IndexingJobs.Enable) { Value = scheduledIndexing });
        settingsManager
            .Setup(x => x.GetObjectSettingAsync(SearchSettings.IndexingJobs.CronExpression.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry(SearchSettings.IndexingJobs.CronExpression) { Value = "0/5 * * * *" });

        return settingsManager.Object;
    }

    // A copy of the category with its outlines and its parent, as CategoryService loads it.
    private static Category LoadCategory(CatalogToolsTestData data, Category category)
    {
        var copy = category.CloneTyped();
        copy.Outlines = data.GetOutlines(category);
        copy.Properties = category.Properties?.ToList() ?? [];
        copy.Parent = category.ParentId == null ? null : LoadCategory(data, data.GetCategory(category.ParentId));

        return copy;
    }

    // A copy of the product with its catalog, category and outlines, as ItemService loads it.
    private static CatalogProduct LoadProduct(CatalogToolsTestData data, CatalogProduct product)
    {
        var copy = product.CloneTyped();
        copy.Catalog = data.GetCatalog(product.CatalogId);
        copy.Category = LoadCategory(data, data.GetCategory(product.CategoryId));
        copy.Outlines = data.GetOutlines(product);
        copy.MainProduct = product.MainProductId == null ? null : data.GetProduct(product.MainProductId);

        return copy;
    }

    private static bool IsUnder(CatalogToolsTestData data, Category category, IEnumerable<string> ancestorIds)
    {
        for (var current = category.ParentId == null ? null : data.GetCategory(category.ParentId); current != null;
             current = current.ParentId == null ? null : data.GetCategory(current.ParentId))
        {
            if (ancestorIds.Contains(current.Id))
            {
                return true;
            }
        }

        return false;
    }
}
