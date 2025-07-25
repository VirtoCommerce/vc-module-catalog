using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public interface ICatalogRepository : IRepository
    {
        IQueryable<CategoryEntity> Categories { get; }
        IQueryable<CatalogEntity> Catalogs { get; }
        IQueryable<ItemEntity> Items { get; }
        IQueryable<PropertyEntity> Properties { get; }
        IQueryable<PropertyGroupEntity> PropertyGroups { get; }
        IQueryable<ImageEntity> Images { get; }
        IQueryable<AssetEntity> Assets { get; }
        IQueryable<VideoEntity> Videos { get; }
        IQueryable<EditorialReviewEntity> EditorialReviews { get; }
        IQueryable<CategoryDescriptionEntity> CategoryDescriptions { get; }
        IQueryable<PropertyValueEntity> PropertyValues { get; }
        IQueryable<PropertyDictionaryValueEntity> PropertyDictionaryValues { get; }
        IQueryable<PropertyDictionaryItemEntity> PropertyDictionaryItems { get; }
        IQueryable<CategoryItemRelationEntity> CategoryItemRelations { get; }
        IQueryable<AssociationEntity> Associations { get; }
        IQueryable<CategoryRelationEntity> CategoryLinks { get; }
        IQueryable<SeoInfoEntity> SeoInfos { get; }
        IQueryable<MeasureEntity> Measures { get; }
        IQueryable<MeasureUnitEntity> MeasureUnits { get; }
        IQueryable<ProductConfigurationEntity> ProductConfigurations { get; }
        IQueryable<ProductConfigurationSectionEntity> ProductConfigurationSections { get; }
        IQueryable<ProductConfigurationOptionEntity> ProductConfigurationOptions { get; }
        IQueryable<AutomaticLinkQueryEntity> AutomaticLinkQueries { get; }

        Task<IList<string>> GetAllSeoDuplicatesIdsAsync();

        Task<IList<string>> GetAllChildrenCategoriesIdsAsync(IList<string> categoryIds);

        Task<IList<CatalogEntity>> GetCatalogsByIdsAsync(IList<string> catalogIds);

        Task<IList<CategoryEntity>> GetCategoriesByIdsAsync(IList<string> categoriesIds, string responseGroup);

        Task<IList<ItemEntity>> GetItemByIdsAsync(IList<string> itemIds, string responseGroup = null);

        Task<IList<PropertyEntity>> GetAllCatalogPropertiesAsync(string catalogId);

        Task<IList<PropertyEntity>> GetPropertiesByIdsAsync(IList<string> propIds, bool loadDictValues = false);

        Task<IList<PropertyGroupEntity>> GetPropertyGroupsByIdsAsync(IList<string> ids, string responseGroup);

        Task<IList<PropertyDictionaryItemEntity>> GetPropertyDictionaryItemsByIdsAsync(IList<string> dictItemIds);

        Task<IList<AssociationEntity>> GetAssociationsByIdsAsync(IList<string> associationIds);

        Task<GenericSearchResult<AssociationEntity>> SearchAssociations(ProductAssociationSearchCriteria criteria);

        Task RemoveItemsAsync(IList<string> itemIds);

        Task RemoveCategoriesAsync(IList<string> ids);

        Task RemoveCatalogsAsync(IList<string> ids);

        Task RemoveAllPropertyValuesAsync(string propertyId);

        Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId);

        Task<IList<MeasureEntity>> GetMeasuresByIdsAsync(IList<string> ids);

        Task<IList<ProductConfigurationEntity>> GetConfigurationsByIdsAsync(IList<string> ids, CancellationToken cancellationToken);

        Task<IList<AutomaticLinkQueryEntity>> GetAutomaticLinkQueriesByIdsAsync(IList<string> ids, string responseGroup);
    }
}
