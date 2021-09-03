using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public interface ICatalogRepositoryForCrud : IRepository
    {
        IQueryable<CategoryEntity> Categories { get; }
        IQueryable<CatalogEntity> Catalogs { get; }
        IQueryable<ItemEntity> Items { get; }
        IQueryable<PropertyEntity> Properties { get; }
        IQueryable<ImageEntity> Images { get; }
        IQueryable<AssetEntity> Assets { get; }
        IQueryable<EditorialReviewEntity> EditorialReviews { get; }
        IQueryable<PropertyValueEntity> PropertyValues { get; }
        IQueryable<PropertyDictionaryValueEntity> PropertyDictionaryValues { get; }
        IQueryable<PropertyDictionaryItemEntity> PropertyDictionaryItems { get; }
        IQueryable<CategoryItemRelationEntity> CategoryItemRelations { get; }
        IQueryable<AssociationEntity> Associations { get; }
        IQueryable<CategoryRelationEntity> CategoryLinks { get; }
        IQueryable<SeoInfoEntity> SeoInfos { get; }

        Task<IEnumerable<string>> GetAllSeoDuplicatesIdsAsync();

        Task<IEnumerable<string>> GetAllChildrenCategoriesIdsAsync(IEnumerable<string> categoryIds);

        Task<IEnumerable<CatalogEntity>> GetCatalogsByIdsAsync(IEnumerable<string> ids, string responseGroup = null);

        Task<IEnumerable<CategoryEntity>> GetCategoriesByIdsAsync(IEnumerable<string> categoriesIds, string responseGroup);

        Task<IEnumerable<ItemEntity>> GetItemByIdsAsync(IEnumerable<string> itemIds, string responseGroup = null);

        Task<IEnumerable<PropertyEntity>> GetAllCatalogPropertiesAsync(string catalogId);

        Task<IEnumerable<PropertyEntity>> GetPropertiesByIdsAsync(IEnumerable<string> propIds, bool loadDictValues = false);

        Task<IEnumerable<PropertyDictionaryItemEntity>> GetPropertyDictionaryItemsByIdsAsync(IEnumerable<string> dictItemIds);

        Task<IEnumerable<AssociationEntity>> GetAssociationsByIdsAsync(IEnumerable<string> associationIds);

        Task RemoveItemsAsync(IEnumerable<string> itemIds);

        Task RemoveCategoriesAsync(IEnumerable<string> ids);

        Task RemoveCatalogsAsync(IEnumerable<string> ids);

        Task RemoveAllPropertyValuesAsync(string propertyId);
    }
}
