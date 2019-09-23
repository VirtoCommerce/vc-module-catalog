using System;
using System.IO;
using System.Web.Http;
using FluentValidation;
using Hangfire.Common;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Extensions;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Security;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Services.OutlineParts;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.CatalogModule.Web.ExportImport;
using VirtoCommerce.CatalogModule.Web.JsonConverters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.ExportModule.Data.Extensions;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using BulkUpdateModel = VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;

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

            _container.RegisterType<ICatalogExportPagedDataSourceFactory, CatalogExportPagedDataSourceFactory>();


            _container.RegisterType<CatalogExportSecurityHandler>();

            #endregion

            #region Search

            _container.RegisterType<IBrowseFilterService, BrowseFilterService>();
            _container.RegisterType<ITermFilterBuilder, TermFilterBuilder>();
            _container.RegisterType<IAggregationConverter, AggregationConverter>();

            _container.RegisterType<ISearchRequestBuilder, ProductSearchRequestBuilder>(nameof(ProductSearchRequestBuilder));
            _container.RegisterType<ISearchRequestBuilder, CategorySearchRequestBuilder>(nameof(CategorySearchRequestBuilder));

            _container.RegisterType<IProductSearchService, ProductSearchService>();
            _container.RegisterType<ICategorySearchService, CategorySearchService>();
            _container.RegisterType<IProperyDictionaryItemService, PropertyDictionaryItemService>();
            _container.RegisterType<IProperyDictionaryItemSearchService, PropertyDictionaryItemService>();


            #endregion

            #region Property Validation

            Func<PropertyValidationRule, PropertyValueValidator> propertyValueValidatorFactory =
                rule => new PropertyValueValidator(rule);

            _container.RegisterInstance(propertyValueValidatorFactory);

            _container.RegisterType<AbstractValidator<IHasProperties>, HasPropertiesValidator>();

            #endregion

            #region Bulk update

            _container.RegisterType<IListEntrySearchService, ListEntrySearchService>();
            _container.RegisterType<IListEntryMover<Category>, CategoryMover>();
            _container.RegisterType<IListEntryMover<CatalogProduct>, ProductMover>();

            _container.RegisterInstance<IBulkUpdateActionRegistrar>(new BulkUpdateActionRegistrar());
            _container.RegisterType<IBulkUpdateActionExecutor, BulkUpdateActionExecutor>();
            _container.RegisterType<IBulkUpdateActionFactory, BulkUpdateActionFactory>();
            _container.RegisterType<IBulkUpdatePropertyManager, BulkUpdatePropertyManager>();
            _container.RegisterType<BulkUpdateModel.IPagedDataSourceFactory, BulkUpdateDataSourceFactory>();

            #endregion Bulk update
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            var securityScopeService = _container.Resolve<IPermissionScopeService>();
            securityScopeService.RegisterSope(() => new CatalogSelectedScope());
            securityScopeService.RegisterSope(() => new CatalogSelectedCategoryScope(_container.Resolve<ICategoryService>()));

            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new SearchCriteriaJsonConverter());
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new BulkUpdateActionContextJsonConverter());

            // This line refreshes Hangfire JsonConverter with the current JsonSerializerSettings - PolymorphicExportDataQueryJsonConverter needs to be included
            JobHelper.SetSerializerSettings(httpConfiguration.Formatters.JsonFormatter.SerializerSettings);

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


            #region Register types for generic Export

            var registrar = _container.Resolve<IKnownExportTypesRegistrar>();

            registrar.RegisterType(
            ExportedTypeDefinitionBuilder.Build<ExportableCatalogFull, CatalogFullExportDataQuery>()
                .WithDataSourceFactory(_container.Resolve<ICatalogExportPagedDataSourceFactory>())
                .WithPermissionAuthorization(CatalogPredefinedPermissions.Export, CatalogPredefinedPermissions.Read)
                .WithMetadata(new ExportedTypeMetadata { PropertyInfos = Array.Empty<ExportedTypePropertyInfo>() })
                .WithAuthorizationHandler(_container.Resolve<CatalogExportSecurityHandler>())
                );

            registrar.RegisterType(
                ExportedTypeDefinitionBuilder.Build<ExportableProduct, ProductExportDataQuery>()
                    .WithDataSourceFactory(_container.Resolve<ICatalogExportPagedDataSourceFactory>())
                    .WithMetadata(typeof(ExportableProduct).GetPropertyNames(
                        nameof(ExportableProduct.Properties),
                        $"{nameof(ExportableProduct.Properties)}.{nameof(Property.Attributes)}",
                        $"{nameof(ExportableProduct.Properties)}.{nameof(Property.DisplayNames)}",
                        $"{nameof(ExportableProduct.Properties)}.{nameof(Property.ValidationRules)}",
                        nameof(ExportableProduct.PropertyValues),
                        nameof(ExportableProduct.Assets),
                        nameof(ExportableProduct.Links),
                        nameof(ExportableProduct.SeoInfos),
                        nameof(ExportableProduct.Reviews),
                        nameof(ExportableProduct.Associations),
                        nameof(ExportableProduct.ReferencedAssociations),
                        nameof(ExportableProduct.Outlines),
                        nameof(ExportableProduct.Images)))
                    .WithTabularMetadata(typeof(ExportableProduct).GetPropertyNames())
                    .WithPermissionAuthorization(CatalogPredefinedPermissions.Export, CatalogPredefinedPermissions.Read)
                    .WithAuthorizationHandler(_container.Resolve<CatalogExportSecurityHandler>()));

            #endregion

            #region Bulk update

            AbstractTypeFactory<BulkUpdateActionContext>.RegisterType<ChangeCategoryActionContext>();
            AbstractTypeFactory<BulkUpdateActionContext>.RegisterType<UpdatePropertiesActionContext>();

            var actionRegistrar = _container.Resolve<IBulkUpdateActionRegistrar>();

            // TechDebt: IItemService and similar does not decorated with vc-module-cache/CatalogServicesDecorator as it is not registered yet.
            // Cache decorator registration is in PostInitialize for all used service being inited.
            // Thus items cache is not invalidated after the changes.
            // So need to handle this situation here. Possible solutions:
            // 1. WithActionFactory and WithDataSourceFactory should use registered creation factory (e.g. Func<IBulkUpdateActionFactory>) for defferred factories instantiation (IMHO preffered)
            // 2. Pass DI container (IUnityContainer) to the factories. (not safe because of potential harmful container usage there)
            // Workaround - turn off Smart caching in platform UI in Settings/Cache/General.
            actionRegistrar.Register(new BulkUpdateActionDefinitionBuilder(
                new BulkUpdateActionDefinition()
                {
                    Name = nameof(ChangeCategoryBulkUpdateAction),
                    AppliableTypes = new[] { nameof(CatalogProduct), },
                    ContextTypeName = nameof(ChangeCategoryActionContext),
                })
                .WithActionFactory(_container.Resolve<IBulkUpdateActionFactory>())
                .WithDataSourceFactory(_container.Resolve<BulkUpdateModel.IPagedDataSourceFactory>())
            );

            actionRegistrar.Register(new BulkUpdateActionDefinitionBuilder(
                new BulkUpdateActionDefinition()
                {
                    Name = nameof(UpdatePropertiesBulkUpdateAction),
                    AppliableTypes = new string[] { nameof(CatalogProduct), },
                    ContextTypeName = nameof(UpdatePropertiesActionContext),
                })
                .WithActionFactory(_container.Resolve<IBulkUpdateActionFactory>())
                .WithDataSourceFactory(_container.Resolve<BulkUpdateModel.IPagedDataSourceFactory>())
            );

            #endregion Bulk update
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
