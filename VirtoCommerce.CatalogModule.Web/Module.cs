using System;
using System.IO;
using System.Web.Http;
using FluentValidation;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Services.OutlineParts;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.CatalogModule.Web.ExportImport;
using VirtoCommerce.CatalogModule.Web.JsonConverters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.CatalogModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly string _connectionString = ConfigurationHelper.GetConnectionStringValue("VirtoCommerce.Catalog") ?? ConfigurationHelper.GetConnectionStringValue("VirtoCommerce");
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            base.SetupDatabase();

            using (var db = new CatalogRepositoryImpl(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CatalogRepositoryImpl, Data.Migrations.Configuration>();
                //The workaround of a known bug with specifying default command timeout within the EF connection string. https://stackoverflow.com/questions/6232633/entity-framework-timeouts/6234593#6234593
                db.Database.CommandTimeout = db.Database.Connection.ConnectionTimeout;
                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            #region Catalog dependencies

            Func<ICatalogRepository> catalogRepFactory = () =>
                new CatalogRepositoryImpl(_connectionString, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                    new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { nameof(ItemEntity), nameof(CategoryEntity) }));

            _container.RegisterInstance(catalogRepFactory);

            _container.RegisterType<IItemService, ItemServiceImpl>();
            _container.RegisterType<ICategoryService, CategoryServiceImpl>();
            _container.RegisterType<ICatalogService, CatalogServiceImpl>();
            _container.RegisterType<IPropertyService, PropertyServiceImpl>();
            _container.RegisterType<ICatalogSearchService, CatalogSearchServiceDecorator>();
            _container.RegisterType<ISkuGenerator, DefaultSkuGenerator>();
            _container.RegisterType<ISeoDuplicatesDetector, CatalogSeoDublicatesDetector>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAssociationService, AssociationServiceImpl>();
            _container.RegisterType<IProductAssociationSearchService, ProductAssociationSearchService>();

            // Detect strategy to use for outline rendering.
            if (_container.Resolve<ISettingsManager>().GetValue("Catalog.CodesInOutline", false))
                _container.RegisterType<IOutlinePartResolver, CodeOutlinePartResolver>();
            else
                _container.RegisterType<IOutlinePartResolver, IdOutlinePartResolver>();

            _container.RegisterType<IOutlineService, OutlineService>();

            #endregion

            #region Search

            _container.RegisterType<IBrowseFilterService, BrowseFilterService>();
            _container.RegisterType<ITermFilterBuilder, TermFilterBuilder>();
            _container.RegisterType<IAggregationConverter, AggregationConverter>();

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
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new SearchCriteriaJsonConverter());

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
    }
}
