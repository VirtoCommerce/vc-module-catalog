using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    /// <summary>
    /// ICatalogRawDatabaseCommand helps to improve performance critical operations by using Raw Database Commands.
    /// </summary>
    public interface ICatalogRawDatabaseCommand
    {
        Task<string[]> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext);
        Task<string[]> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, string[] categoryIds);
        Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria);
        Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId);

        Task RemoveItemsAsync(CatalogDbContext dbContext, string[] itemIds);
        Task RemoveCategoriesAsync(CatalogDbContext dbContext, string[] ids);
        Task RemoveCatalogsAsync(CatalogDbContext dbContext, string[] ids);
        Task RemoveAllPropertyValuesAsync(CatalogDbContext dbContext, PropertyEntity catalogProperty, PropertyEntity categoryProperty, PropertyEntity itemProperty);
    }
}
