using System;
using System.IO;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.ExportImport;
using VirtoCommerce.CatalogModule.Web.JsonConverters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Search;
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

            using (var db = new CatalogRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CatalogRepositoryImpl, Data.Migrations.Configuration>();

                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            #region Catalog dependencies

            Func<ICatalogRepository> catalogRepFactory = () =>
                new CatalogRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                    new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { typeof(Item).Name }, _container.Resolve<IUserNameResolver>()));

            _container.RegisterInstance(catalogRepFactory);

            _container.RegisterType<IItemService, ItemServiceImpl>();
            _container.RegisterType<ICategoryService, CategoryServiceImpl>();
            _container.RegisterType<ICatalogService, CatalogServiceImpl>();
            _container.RegisterType<IPropertyService, PropertyServiceImpl>();
            _container.RegisterType<ICatalogSearchService, CatalogSearchServiceImpl>();
            _container.RegisterType<ISkuGenerator, DefaultSkuGenerator>();
            _container.RegisterType<ISeoDuplicatesDetector, CatalogSeoDublicatesDetector>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IOutlineService, OutlineService>();
            _container.RegisterType<IAssociationService, AssociationServiceImpl>();

            #endregion

            #region Search

            _container.RegisterType<IBrowseFilterService, BrowseFilterService>();

            _container.RegisterType<IProductSearchService, ProductSearchService>();
            _container.RegisterType<ICategorySearchService, CategorySearchService>();

            _container.RegisterType<ISearchRequestBuilder, ProductSearchRequestBuilder>(nameof(ProductSearchRequestBuilder));
            _container.RegisterType<ISearchRequestBuilder, CategorySearchRequestBuilder>(nameof(CategorySearchRequestBuilder));

            var productIndexingConfiguration = new IndexDocumentConfiguration
            {
                DocumentType = Constants.ProductDocumentType,
                DocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _container.Resolve<ProductDocumentChangesProvider>(),
                    DocumentBuilder = _container.Resolve<ProductDocumentBuilder>(),
                },
            };

            _container.RegisterInstance(productIndexingConfiguration.DocumentType, productIndexingConfiguration);

            #endregion
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            var securityScopeService = _container.Resolve<IPermissionScopeService>();
            securityScopeService.RegisterSope(() => new CatalogSelectedScope());
            securityScopeService.RegisterSope(() => new CatalogSelectedCategoryScope(_container.Resolve<ICategoryService>()));

            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new ProductSearchJsonConverter());
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new CategorySearchJsonConverter());
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
