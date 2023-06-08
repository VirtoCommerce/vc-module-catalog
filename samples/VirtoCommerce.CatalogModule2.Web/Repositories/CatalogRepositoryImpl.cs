using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Repositories
{
    public class CatalogRepositoryImpl2 : CatalogRepositoryImpl
    {
        public CatalogRepositoryImpl2(CatalogDbContext dbContext, ICatalogRawDatabaseCommand rawDatabaseCommand)
            : base(dbContext, rawDatabaseCommand)
        {
        }

        public override Task<IList<PropertyEntity>> GetAllCatalogPropertiesAsync(string catalogId)
        {
            return base.GetAllCatalogPropertiesAsync(catalogId);
        }
        public override Task<IList<string>> GetAllChildrenCategoriesIdsAsync(IList<string> categoryIds)
        {
            return base.GetAllChildrenCategoriesIdsAsync(categoryIds);
        }
        public override Task<IList<string>> GetAllSeoDuplicatesIdsAsync()
        {
            return base.GetAllSeoDuplicatesIdsAsync();
        }
        public override Task<IList<AssociationEntity>> GetAssociationsByIdsAsync(IList<string> associationIds)
        {
            return base.GetAssociationsByIdsAsync(associationIds);
        }
        public override Task<IList<CatalogEntity>> GetCatalogsByIdsAsync(IList<string> catalogIds)
        {
            return base.GetCatalogsByIdsAsync(catalogIds);
        }
        public override Task<IList<CategoryEntity>> GetCategoriesByIdsAsync(IList<string> categoriesIds, string responseGroup)
        {
            return base.GetCategoriesByIdsAsync(categoriesIds, responseGroup);
        }
        public override Task<IList<ItemEntity>> GetItemByIdsAsync(IList<string> itemIds, string responseGroup = null)
        {
            return base.GetItemByIdsAsync(itemIds, responseGroup);
        }
        public override Task<IList<PropertyEntity>> GetPropertiesByIdsAsync(IList<string> propIds, bool loadDictValues = false)
        {
            return base.GetPropertiesByIdsAsync(propIds, loadDictValues);
        }
        public override Task<IList<PropertyDictionaryItemEntity>> GetPropertyDictionaryItemsByIdsAsync(IList<string> dictItemIds)
        {
            return base.GetPropertyDictionaryItemsByIdsAsync(dictItemIds);
        }
        public override Task RemoveAllPropertyValuesAsync(string propertyId)
        {
            return base.RemoveAllPropertyValuesAsync(propertyId);
        }
        public override Task RemoveCatalogsAsync(IList<string> ids)
        {
            return base.RemoveCatalogsAsync(ids);
        }
        public override Task RemoveCategoriesAsync(IList<string> ids)
        {
            return base.RemoveCategoriesAsync(ids);
        }
        public override Task RemoveItemsAsync(IList<string> itemIds)
        {
            return base.RemoveItemsAsync(itemIds);
        }
        public override Task<GenericSearchResult<AssociationEntity>> SearchAssociations(ProductAssociationSearchCriteria criteria)
        {
            return base.SearchAssociations(criteria);
        }
        public override Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            return base.SearchCategoriesHierarchyAsync(categoryId);
        }
    }
}
