using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Export;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule2.Core.Model;
using VirtoCommerce.CatalogModule2.Core.Model.Export;
using VirtoCommerce.CatalogModule2.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule2.Core.Model.Search;
using VirtoCommerce.CatalogModule2.Data.Model;
using VirtoCommerce.CatalogModule2.Data.Repositories;
using VirtoCommerce.CatalogModule2.Data.Search;
using VirtoCommerce.CatalogModule2.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule2.Data.Search.Indexing;
using VirtoCommerce.CatalogModule2.Data.Services;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule2.Web
{
    /// <summary>
    /// This module demonstrates how to extend CatalogModule
    /// Try to override most parts of the module
    /// </summary>
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICatalogRepository, CatalogRepositoryImpl2>();

            serviceCollection.AddTransient<IProductSearchService, ProductSearchService2>();
            serviceCollection.AddTransient<ICategorySearchService, CategorySearchService2>();

            serviceCollection.AddTransient<ICatalogService, CatalogService2>();
            serviceCollection.AddTransient<ICatalogSearchService, CatalogSearchService2>();
            serviceCollection.AddTransient<IListEntrySearchService, ListEntrySearchService2>();

            serviceCollection.AddTransient<ICategoryService, CategoryService2>();
            serviceCollection.AddTransient<ICategoryIndexedSearchService, CategoryIndexedSearchService>();

            serviceCollection.AddTransient<IItemService, ItemService2>();
            serviceCollection.AddTransient<IProductIndexedSearchService, ProductIndexedSearchService2>();
            serviceCollection.AddTransient<IAssociationService, AssociationService2>();

            serviceCollection.AddTransient<IVideoSearchService, VideoSearchService2>();
            serviceCollection.AddTransient<IVideoService, VideoService2>();

            serviceCollection.AddTransient<IAggregationConverter, AggregationConverter2>();
            serviceCollection.AddTransient<IBrowseFilterService, BrowseFilterService2>();
            serviceCollection.AddTransient<ITermFilterBuilder, TermFilterBuilder2>();

            serviceCollection.AddTransient<IPropertyService, PropertyService2>();
            serviceCollection.AddTransient<IPropertySearchService, PropertySearchService2>();

            serviceCollection.AddTransient<IPropertyDictionaryItemService, PropertyDictionaryItemService2>();
            serviceCollection.AddTransient<IPropertyDictionaryItemSearchService, PropertyDictionaryItemSearchService2>();

            serviceCollection.AddTransient<IProductAssociationSearchService, ProductAssociationSearchService2>();

            serviceCollection.AddTransient<ISeoBySlugResolver, CatalogSeoBySlugResolver2>();

            serviceCollection.AddTransient<IInternalListEntrySearchService, InternalListEntrySearchService2>();

            serviceCollection.AddTransient<ISeoDuplicatesDetector, CatalogSeoDuplicatesDetector2>();

            serviceCollection.AddTransient<ListEntryMover<Category>, CategoryMover2>();
            serviceCollection.AddTransient<ListEntryMover<CatalogProduct>, ProductMover2>();

        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            AbstractTypeFactory<Asset>.OverrideType<Asset, Asset2>();
            AbstractTypeFactory<Catalog>.OverrideType<Catalog, Catalog2>();
            AbstractTypeFactory<CatalogProduct>.OverrideType<CatalogProduct, CatalogProduct2>();
            AbstractTypeFactory<Category>.OverrideType<Category, Category2>();
            AbstractTypeFactory<CategoryDescription>.OverrideType<CategoryDescription, CategoryDescription2>();
            AbstractTypeFactory<EditorialReview>.OverrideType<EditorialReview, EditorialReview2>();
            AbstractTypeFactory<Image>.OverrideType<Image, Image2>();
            AbstractTypeFactory<ProductAssociation>.OverrideType<ProductAssociation, ProductAssociation2>();
            AbstractTypeFactory<Property>.OverrideType<Property, Property2>();
            AbstractTypeFactory<PropertyValue>.OverrideType<PropertyValue, PropertyValue2>();
            AbstractTypeFactory<Variation>.OverrideType<Variation, Variation2>();
            AbstractTypeFactory<ExportableProduct>.OverrideType<ExportableProduct, ExportableProduct2>();
            AbstractTypeFactory<CategoryListEntry>.OverrideType<CategoryListEntry, CategoryListEntry2>();
            AbstractTypeFactory<ProductListEntry>.OverrideType<ProductListEntry, ProductListEntry2>();
            AbstractTypeFactory<CatalogIndexedSearchCriteria>.OverrideType<CatalogIndexedSearchCriteria, CatalogIndexedSearchCriteria2>();
            AbstractTypeFactory<CategoryIndexedSearchCriteria>.OverrideType<CategoryIndexedSearchCriteria, CategoryIndexedSearchCriteria2>();
            AbstractTypeFactory<FiltersContainer>.OverrideType<FiltersContainer, FiltersContainer2>();
            AbstractTypeFactory<ProductIndexedSearchCriteria>.OverrideType<ProductIndexedSearchCriteria, ProductIndexedSearchCriteria2>();
            AbstractTypeFactory<AssetEntity>.OverrideType<AssetEntity, AssetEntity2>();
            AbstractTypeFactory<AssociationEntity>.OverrideType<AssociationEntity, AssociationEntity2>();
            AbstractTypeFactory<CatalogEntity>.OverrideType<CatalogEntity, CatalogEntity2>();
            AbstractTypeFactory<CatalogLanguageEntity>.OverrideType<CatalogLanguageEntity, CatalogLanguageEntity2>();
            AbstractTypeFactory<CategoryDescriptionEntity>.OverrideType<CategoryDescriptionEntity, CategoryDescriptionEntity2>();
            AbstractTypeFactory<CategoryEntity>.OverrideType<CategoryEntity, CategoryEntity2>();
            AbstractTypeFactory<CategoryItemRelationEntity>.OverrideType<CategoryItemRelationEntity, CategoryItemRelationEntity2>();
            AbstractTypeFactory<CategoryRelationEntity>.OverrideType<CategoryRelationEntity, CategoryRelationEntity2>();
            AbstractTypeFactory<EditorialReviewEntity>.OverrideType<EditorialReviewEntity, EditorialReviewEntity2>();
            AbstractTypeFactory<ImageEntity>.OverrideType<ImageEntity, ImageEntity2>();
            AbstractTypeFactory<ItemEntity>.OverrideType<ItemEntity, ItemEntity2>();
            AbstractTypeFactory<PropertyAttributeEntity>.OverrideType<PropertyAttributeEntity, PropertyAttributeEntity2>();
            AbstractTypeFactory<PropertyDictionaryItemEntity>.OverrideType<PropertyDictionaryItemEntity, PropertyDictionaryItemEntity2>();
            AbstractTypeFactory<PropertyDictionaryValueEntity>.OverrideType<PropertyDictionaryValueEntity, PropertyDictionaryValueEntity2>();
            AbstractTypeFactory<PropertyDisplayNameEntity>.OverrideType<PropertyDisplayNameEntity, PropertyDisplayNameEntity2>();
            AbstractTypeFactory<PropertyEntity>.OverrideType<PropertyEntity, PropertyEntity2>();
            AbstractTypeFactory<PropertyValidationRuleEntity>.OverrideType<PropertyValidationRuleEntity, PropertyValidationRuleEntity2>();
            AbstractTypeFactory<PropertyValueEntity>.OverrideType<PropertyValueEntity, PropertyValueEntity2>();
            AbstractTypeFactory<SeoInfoEntity>.OverrideType<SeoInfoEntity, SeoInfoEntity2>();
            AbstractTypeFactory<VideoEntity>.OverrideType<VideoEntity, VideoEntity2>();

        }

        public void Uninstall()
        {
            // No needed
        }
    }
}
