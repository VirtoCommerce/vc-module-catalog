using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using FluentValidation;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogModule.Data.Infrastructure.Interceptors;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.ExportImport;
using VirtoCommerce.CatalogModule.Web.JsonConverters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            base.SetupDatabase();

            using (var db = new CatalogRepositoryImpl(null, _connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CatalogRepositoryImpl, Data.Migrations.Configuration>();

                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            #region Catalog dependencies

            var interceptors = new List<IInterceptor>
            {
                new EntityPrimaryKeyGeneratorInterceptor(),
                _container.Resolve<AuditableInterceptor>(),
                new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative,
                    new[] {nameof(ItemEntity), nameof(CategoryEntity)}, _container.Resolve<IUserNameResolver>()),
                new NearRealtimeIndexer(TryResolve<ISearchProvider>, _container.Resolve<ISettingsManager>(), TryResolve<IIndexingManager>)
            };

            Func<ICatalogRepository> catalogRepFactory = () => new CatalogRepositoryImpl(TryResolve<ISearchProvider>, _connectionStringName, interceptors.ToArray());

            _container.RegisterInstance(catalogRepFactory);

            _container.RegisterType<IItemService, ItemServiceImpl>();
            _container.RegisterType<ICategoryService, CategoryServiceImpl>();
            _container.RegisterType<ICatalogService, CatalogServiceImpl>();
            _container.RegisterType<IPropertyService, PropertyServiceImpl>();
            _container.RegisterType<ICatalogSearchService, CatalogSearchServiceDecorator>();
            _container.RegisterType<ISkuGenerator, DefaultSkuGenerator>();
            _container.RegisterType<ISeoDuplicatesDetector, CatalogSeoDublicatesDetector>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IOutlineService, OutlineService>();
            _container.RegisterType<IAssociationService, AssociationServiceImpl>();

            #endregion

            #region Search

            _container.RegisterType<IBrowseFilterService, BrowseFilterService>();

            _container.RegisterType<ISearchRequestBuilder, ProductSearchRequestBuilder>(nameof(ProductSearchRequestBuilder));
            _container.RegisterType<ISearchRequestBuilder, CategorySearchRequestBuilder>(nameof(CategorySearchRequestBuilder));

            _container.RegisterType<IProductSearchService, ProductSearchService>();
            _container.RegisterType<ICategorySearchService, CategorySearchService>();

            #endregion

            #region Property Validation

            Func<PropertyValidationRule, PropertyValueValidator> propertyValueValidatorFactory =
                rule => new PropertyValueValidator(rule);

            _container.RegisterInstance(propertyValueValidatorFactory);

            _container.RegisterType<AbstractValidator<IHasProperties>, HasPropertiesValidator>();
            
            #endregion
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            var securityScopeService = _container.Resolve<IPermissionScopeService>();
            securityScopeService.RegisterSope(() => new CatalogSelectedScope());
            securityScopeService.RegisterSope(() => new CatalogSelectedCategoryScope(_container.Resolve<ICategoryService>()));

            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new ProductSearchCriteriaJsonConverter());
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new CategorySearchCriteriaJsonConverter());

            // Register dynamic property for storing browsing filters
            var filteredBrowsingProperty = new DynamicProperty
            {
                Id = "2b15f370ab524186bec1ace82509a60a",
                Name = BrowseFilterService.FilteredBrowsingPropertyName,
                ObjectType = typeof(Store).FullName,
                ValueType = DynamicPropertyValueType.LongText,
                CreatedBy = "Auto"
            };

            var dynamicPropertyService = _container.Resolve<IDynamicPropertyService>();
            dynamicPropertyService.SaveProperties(new[] { filteredBrowsingProperty });

            // Product indexing configuration
            var productIndexingConfiguration = new IndexDocumentConfiguration
            {
                DocumentType = KnownDocumentTypes.Product,
                DocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _container.Resolve<ProductDocumentChangesProvider>(),
                    DocumentBuilder = _container.Resolve<ProductDocumentBuilder>(),
                },
            };

            _container.RegisterInstance(productIndexingConfiguration.DocumentType, productIndexingConfiguration);

            // Category indexing configuration
            var categoryIndexingConfiguration = new IndexDocumentConfiguration
            {
                DocumentType = KnownDocumentTypes.Category,
                DocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _container.Resolve<CategoryDocumentChangesProvider>(),
                    DocumentBuilder = _container.Resolve<CategoryDocumentBuilder>(),
                },
            };

            _container.RegisterInstance(categoryIndexingConfiguration.DocumentType, categoryIndexingConfiguration);
        }

        #endregion

        #region ISupportExportImportModule Members

        public void DoExport(Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CatalogExportImport>();
            exportJob.DoExport(outStream, manifest, progressCallback);
        }

        public void DoImport(Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CatalogExportImport>();
            exportJob.DoImport(inputStream, manifest, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Catalog.ExportImport.Description", string.Empty);
            }
        }

        #endregion

        private T TryResolve<T>()
            where T : class
        {
            try
            {
                return _container.Resolve<T>();
            }
            catch (Exception)
            {
                return (T)null;
            }
        }
    }
}
