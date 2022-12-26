using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.MySql
{
    public class MySqlCatalogRawDatabaseCommand : ICatalogRawDatabaseCommand
    {
        public Task<string[]> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, string[] categoryIds)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext)
        {
            throw new NotImplementedException();
        }

        public Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllPropertyValuesAsync(CatalogDbContext dbContext, PropertyEntity catalogProperty, PropertyEntity categoryProperty, PropertyEntity itemProperty)
        {
            throw new NotImplementedException();
        }

        public Task RemoveCatalogsAsync(CatalogDbContext dbContext, string[] ids)
        {
            throw new NotImplementedException();
        }

        public Task RemoveCategoriesAsync(CatalogDbContext dbContext, string[] ids)
        {
            throw new NotImplementedException();
        }

        public Task RemoveItemsAsync(CatalogDbContext dbContext, string[] itemIds)
        {
            throw new NotImplementedException();
        }

    }
}
