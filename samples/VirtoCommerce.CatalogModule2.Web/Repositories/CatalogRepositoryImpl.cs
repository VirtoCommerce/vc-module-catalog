using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Repositories
{
    public class CatalogRepositoryImpl2 : CatalogRepositoryImpl
    {
        public CatalogRepositoryImpl2(CatalogDbContext dbContext) : base(dbContext)
        {
        }

        protected override void AddAssociationsSearchCriteraToCommand(StringBuilder command, ProductAssociationSearchCriteria criteria)
        {
            base.AddAssociationsSearchCriteraToCommand(command, criteria);
        }
        protected override Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            return base.CreateCommand(commandTemplate, parameterValues);
        }
        protected override Task<int> ExecuteStoreQueryAsync(string commandTemplate, IEnumerable<string> parameterValues)
        {
            return base.ExecuteStoreQueryAsync(commandTemplate, parameterValues);
        }
        public override Task<PropertyEntity[]> GetAllCatalogPropertiesAsync(string catalogId)
        {
            return base.GetAllCatalogPropertiesAsync(catalogId);
        }
        public override Task<string[]> GetAllChildrenCategoriesIdsAsync(string[] categoryIds)
        {
            return base.GetAllChildrenCategoriesIdsAsync(categoryIds);
        }
        public override Task<string[]> GetAllSeoDuplicatesIdsAsync()
        {
            return base.GetAllSeoDuplicatesIdsAsync();
        }
        public override Task<AssociationEntity[]> GetAssociationsByIdsAsync(string[] associationIds)
        {
            return base.GetAssociationsByIdsAsync(associationIds);
        }
        protected override string GetAssociationsCountSqlCommandText(ProductAssociationSearchCriteria criteria)
        {
            return base.GetAssociationsCountSqlCommandText(criteria);
        }
        protected override string GetAssociationsQuerySqlCommandText(ProductAssociationSearchCriteria criteria)
        {
            return base.GetAssociationsQuerySqlCommandText(criteria);
        }
        public override Task<CatalogEntity[]> GetCatalogsByIdsAsync(string[] catalogIds)
        {
            return base.GetCatalogsByIdsAsync(catalogIds);
        }
        public override Task<CategoryEntity[]> GetCategoriesByIdsAsync(string[] categoriesIds, string responseGroup)
        {
            return base.GetCategoriesByIdsAsync(categoriesIds, responseGroup);
        }
        public override Task<ItemEntity[]> GetItemByIdsAsync(string[] itemIds, string responseGroup = null)
        {
            return base.GetItemByIdsAsync(itemIds, responseGroup);
        }
        public override Task<PropertyEntity[]> GetPropertiesByIdsAsync(string[] propIds, bool loadDictValues = false)
        {
            return base.GetPropertiesByIdsAsync(propIds, loadDictValues);
        }
        public override Task<PropertyDictionaryItemEntity[]> GetPropertyDictionaryItemsByIdsAsync(string[] dictItemIds)
        {
            return base.GetPropertyDictionaryItemsByIdsAsync(dictItemIds);
        }
        public override Task RemoveAllPropertyValuesAsync(string propertyId)
        {
            return base.RemoveAllPropertyValuesAsync(propertyId);
        }
        public override Task RemoveCatalogsAsync(string[] ids)
        {
            return base.RemoveCatalogsAsync(ids);
        }
        public override Task RemoveCategoriesAsync(string[] ids)
        {
            return base.RemoveCategoriesAsync(ids);
        }
        public override Task RemoveItemsAsync(string[] itemIds)
        {
            return base.RemoveItemsAsync(itemIds);
        }
        public override Task<GenericSearchResult<AssociationEntity>> SearchAssociations(ProductAssociationSearchCriteria criteria)
        {
            return base.SearchAssociations(criteria);
        }
        public override Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            return base.SearchCategoriesHierarchyAsync(categoryId);
        }
    }
}
