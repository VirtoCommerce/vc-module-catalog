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
        /// <summary>
        /// Gets duplicated seo details from CatalogSeoInfo, if it has more than one Keyword and StoreId combination.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns>The array of CatalogSeoInfo Id</returns>
        Task<IList<string>> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext);
        /// <summary>
        /// Gets ids of all children categories recursively for specific array of parent category ids.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="categoryIds"></param>
        /// <returns>The array of Category Id</returns>
        Task<IList<string>> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, IList<string> categoryIds);

        /// <summary>
        /// Searches associations by specific ObjectIds and search criteria.
        /// It should support retrieving: Items and TotalCount.
        /// It should support pagination: Skip and Take.
        /// It should support filters: multiple ObjectIds, Group, multiple Tags (split by ';'), Like by Keyword and multiple AssociatedObjectIds.
        /// </summary>
        /// <param name="dbContext"></param> 
        /// <param name="criteria"></param>
        /// <returns>Returns SearchResult total count and items.</returns>
        Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria);

        /// <summary>
        /// Returns requested category and all its parent categories to the root (up the hierarchy tree) in a plain list.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId);

        /// <summary>
        /// Removes catalog items and related items from database by array of item id. Removes data from: CatalogSeoInfo, CategoryItemRelation, CatalogImage, CatalogAsset, PropertyValue, EditorialReview, Association and Item.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        Task RemoveItemsAsync(CatalogDbContext dbContext, IList<string> itemIds);

        /// <summary>
        /// Removes category and all related items from database by array of catalog id. Removes data from: CatalogSeoInfo, CatalogImage, PropertyValue, CategoryRelation, CategoryItemRelation, Association, Property, CategoryDescription and Category.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task RemoveCategoriesAsync(CatalogDbContext dbContext, IList<string> ids);

        /// <summary>
        /// Removes catalogs and all related items from database by array of catalog id. Removes data from: CatalogLanguage, CategoryRelation, PropertyValue, Property and Catalog.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task RemoveCatalogsAsync(CatalogDbContext dbContext, IList<string> ids);
        /// <summary>
        /// Removes specified catalogProperty, categoryProperty and itemProperty from database.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="catalogProperty"></param>
        /// <param name="categoryProperty"></param>
        /// <param name="itemProperty"></param>
        /// <returns></returns>
        Task RemoveAllPropertyValuesAsync(CatalogDbContext dbContext, PropertyEntity catalogProperty, PropertyEntity categoryProperty, PropertyEntity itemProperty);
    }
}
