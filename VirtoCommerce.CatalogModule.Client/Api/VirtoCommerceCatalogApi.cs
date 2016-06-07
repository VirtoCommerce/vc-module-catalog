using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using VirtoCommerce.CatalogModule.Client.Client;
using VirtoCommerce.CatalogModule.Client.Model;

namespace VirtoCommerce.CatalogModule.Client.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IVirtoCommerceCatalogApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Creates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Creates the specified catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Catalog</returns>
        Catalog CatalogModuleCatalogsCreate(Catalog catalog);

        /// <summary>
        /// Creates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Creates the specified catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>ApiResponse of Catalog</returns>
        ApiResponse<Catalog> CatalogModuleCatalogsCreateWithHttpInfo(Catalog catalog);
        /// <summary>
        /// Deletes catalog by id.
        /// </summary>
        /// <remarks>
        /// Deletes catalog by id
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns></returns>
        void CatalogModuleCatalogsDelete(string id);

        /// <summary>
        /// Deletes catalog by id.
        /// </summary>
        /// <remarks>
        /// Deletes catalog by id
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleCatalogsDeleteWithHttpInfo(string id);
        /// <summary>
        /// Gets Catalog by id.
        /// </summary>
        /// <remarks>
        /// Gets Catalog by id with full information loaded
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Catalog</returns>
        Catalog CatalogModuleCatalogsGet(string id);

        /// <summary>
        /// Gets Catalog by id.
        /// </summary>
        /// <remarks>
        /// Gets Catalog by id with full information loaded
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>ApiResponse of Catalog</returns>
        ApiResponse<Catalog> CatalogModuleCatalogsGetWithHttpInfo(string id);
        /// <summary>
        /// Get Catalogs list
        /// </summary>
        /// <remarks>
        /// Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Catalog&gt;</returns>
        List<Catalog> CatalogModuleCatalogsGetCatalogs();

        /// <summary>
        /// Get Catalogs list
        /// </summary>
        /// <remarks>
        /// Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Catalog&gt;</returns>
        ApiResponse<List<Catalog>> CatalogModuleCatalogsGetCatalogsWithHttpInfo();
        /// <summary>
        /// Gets the template for a new catalog.
        /// </summary>
        /// <remarks>
        /// Gets the template for a new common catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Catalog</returns>
        Catalog CatalogModuleCatalogsGetNewCatalog();

        /// <summary>
        /// Gets the template for a new catalog.
        /// </summary>
        /// <remarks>
        /// Gets the template for a new common catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Catalog</returns>
        ApiResponse<Catalog> CatalogModuleCatalogsGetNewCatalogWithHttpInfo();
        /// <summary>
        /// Gets the template for a new virtual catalog.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Catalog</returns>
        Catalog CatalogModuleCatalogsGetNewVirtualCatalog();

        /// <summary>
        /// Gets the template for a new virtual catalog.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Catalog</returns>
        ApiResponse<Catalog> CatalogModuleCatalogsGetNewVirtualCatalogWithHttpInfo();
        /// <summary>
        /// Updates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Updates the specified catalog.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns></returns>
        void CatalogModuleCatalogsUpdate(Catalog catalog);

        /// <summary>
        /// Updates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Updates the specified catalog.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleCatalogsUpdateWithHttpInfo(Catalog catalog);
        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>
        /// If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        void CatalogModuleCategoriesCreateOrUpdateCategory(Category category);

        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>
        /// If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleCategoriesCreateOrUpdateCategoryWithHttpInfo(Category category);
        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns></returns>
        void CatalogModuleCategoriesDelete(List<string> ids);

        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleCategoriesDeleteWithHttpInfo(List<string> ids);
        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Category</returns>
        Category CatalogModuleCategoriesGet(string id);

        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>ApiResponse of Category</returns>
        ApiResponse<Category> CatalogModuleCategoriesGetWithHttpInfo(string id);
        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>List&lt;Category&gt;</returns>
        List<Category> CatalogModuleCategoriesGetCategoriesByIds(List<string> ids, string respGroup = null);

        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of List&lt;Category&gt;</returns>
        ApiResponse<List<Category>> CatalogModuleCategoriesGetCategoriesByIdsWithHttpInfo(List<string> ids, string respGroup = null);
        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Category</returns>
        Category CatalogModuleCategoriesGetNewCategory(string catalogId, string parentCategoryId = null);

        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>ApiResponse of Category</returns>
        ApiResponse<Category> CatalogModuleCategoriesGetNewCategoryWithHttpInfo(string catalogId, string parentCategoryId = null);
        /// <summary>
        /// Start catalog data export process.
        /// </summary>
        /// <remarks>
        /// Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>ExportNotification</returns>
        ExportNotification CatalogModuleExportImportDoExport(CsvExportInfo exportInfo);

        /// <summary>
        /// Start catalog data export process.
        /// </summary>
        /// <remarks>
        /// Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>ApiResponse of ExportNotification</returns>
        ApiResponse<ExportNotification> CatalogModuleExportImportDoExportWithHttpInfo(CsvExportInfo exportInfo);
        /// <summary>
        /// Start catalog data import process.
        /// </summary>
        /// <remarks>
        /// Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>ImportNotification</returns>
        ImportNotification CatalogModuleExportImportDoImport(CsvImportInfo importInfo);

        /// <summary>
        /// Start catalog data import process.
        /// </summary>
        /// <remarks>
        /// Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>ApiResponse of ImportNotification</returns>
        ApiResponse<ImportNotification> CatalogModuleExportImportDoImportWithHttpInfo(CsvImportInfo importInfo);
        /// <summary>
        /// Gets the CSV mapping configuration.
        /// </summary>
        /// <remarks>
        /// Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>CsvProductMappingConfiguration</returns>
        CsvProductMappingConfiguration CatalogModuleExportImportGetMappingConfiguration(string fileUrl, string delimiter = null);

        /// <summary>
        /// Gets the CSV mapping configuration.
        /// </summary>
        /// <remarks>
        /// Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>ApiResponse of CsvProductMappingConfiguration</returns>
        ApiResponse<CsvProductMappingConfiguration> CatalogModuleExportImportGetMappingConfigurationWithHttpInfo(string fileUrl, string delimiter = null);
        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns></returns>
        void CatalogModuleListEntryCreateLinks(List<ListEntryLink> links);

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleListEntryCreateLinksWithHttpInfo(List<ListEntryLink> links);
        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns></returns>
        void CatalogModuleListEntryDeleteLinks(List<ListEntryLink> links);

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleListEntryDeleteLinksWithHttpInfo(List<ListEntryLink> links);
        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ListEntrySearchResult</returns>
        ListEntrySearchResult CatalogModuleListEntryListItemsSearch(SearchCriteria criteria);

        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ApiResponse of ListEntrySearchResult</returns>
        ApiResponse<ListEntrySearchResult> CatalogModuleListEntryListItemsSearchWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns></returns>
        void CatalogModuleListEntryMove(MoveInfo moveInfo);

        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleListEntryMoveWithHttpInfo(MoveInfo moveInfo);
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Product</returns>
        Product CatalogModuleProductsCloneProduct(string productId);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>ApiResponse of Product</returns>
        ApiResponse<Product> CatalogModuleProductsCloneProductWithHttpInfo(string productId);
        /// <summary>
        /// Deletes the specified items by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns></returns>
        void CatalogModuleProductsDelete(List<string> ids);

        /// <summary>
        /// Deletes the specified items by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleProductsDeleteWithHttpInfo(List<string> ids);
        /// <summary>
        /// Gets the template for a new product (outside of category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog directly.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Product</returns>
        Product CatalogModuleProductsGetNewProductByCatalog(string catalogId);

        /// <summary>
        /// Gets the template for a new product (outside of category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog directly.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>ApiResponse of Product</returns>
        ApiResponse<Product> CatalogModuleProductsGetNewProductByCatalogWithHttpInfo(string catalogId);
        /// <summary>
        /// Gets the template for a new product (inside category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog category.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Product</returns>
        Product CatalogModuleProductsGetNewProductByCatalogAndCategory(string catalogId, string categoryId);

        /// <summary>
        /// Gets the template for a new product (inside category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog category.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>ApiResponse of Product</returns>
        ApiResponse<Product> CatalogModuleProductsGetNewProductByCatalogAndCategoryWithHttpInfo(string catalogId, string categoryId);
        /// <summary>
        /// Gets the template for a new variation.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Product</returns>
        Product CatalogModuleProductsGetNewVariation(string productId);

        /// <summary>
        /// Gets the template for a new variation.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>ApiResponse of Product</returns>
        ApiResponse<Product> CatalogModuleProductsGetNewVariationWithHttpInfo(string productId);
        /// <summary>
        /// Gets product by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Product</returns>
        Product CatalogModuleProductsGetProductById(string id, string respGroup = null);

        /// <summary>
        /// Gets product by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of Product</returns>
        ApiResponse<Product> CatalogModuleProductsGetProductByIdWithHttpInfo(string id, string respGroup = null);
        /// <summary>
        /// Gets products by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>List&lt;Product&gt;</returns>
        List<Product> CatalogModuleProductsGetProductByIds(List<string> ids, string respGroup = null);

        /// <summary>
        /// Gets products by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of List&lt;Product&gt;</returns>
        ApiResponse<List<Product>> CatalogModuleProductsGetProductByIdsWithHttpInfo(List<string> ids, string respGroup = null);
        /// <summary>
        /// Updates the specified product.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns></returns>
        void CatalogModuleProductsUpdate(Product product);

        /// <summary>
        /// Updates the specified product.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModuleProductsUpdateWithHttpInfo(Product product);
        /// <summary>
        /// Creates or updates the specified property.
        /// </summary>
        /// <remarks>
        /// If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        void CatalogModulePropertiesCreateOrUpdateProperty(Property property);

        /// <summary>
        /// Creates or updates the specified property.
        /// </summary>
        /// <remarks>
        /// If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModulePropertiesCreateOrUpdatePropertyWithHttpInfo(Property property);
        /// <summary>
        /// Deletes property by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns></returns>
        void CatalogModulePropertiesDelete(string id);

        /// <summary>
        /// Deletes property by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CatalogModulePropertiesDeleteWithHttpInfo(string id);
        /// <summary>
        /// Gets property metainformation by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Property</returns>
        Property CatalogModulePropertiesGet(string propertyId);

        /// <summary>
        /// Gets property metainformation by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>ApiResponse of Property</returns>
        ApiResponse<Property> CatalogModulePropertiesGetWithHttpInfo(string propertyId);
        /// <summary>
        /// Gets the template for a new catalog property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Property</returns>
        Property CatalogModulePropertiesGetNewCatalogProperty(string catalogId);

        /// <summary>
        /// Gets the template for a new catalog property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>ApiResponse of Property</returns>
        ApiResponse<Property> CatalogModulePropertiesGetNewCatalogPropertyWithHttpInfo(string catalogId);
        /// <summary>
        /// Gets the template for a new category property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Property</returns>
        Property CatalogModulePropertiesGetNewCategoryProperty(string categoryId);

        /// <summary>
        /// Gets the template for a new category property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>ApiResponse of Property</returns>
        ApiResponse<Property> CatalogModulePropertiesGetNewCategoryPropertyWithHttpInfo(string categoryId);
        /// <summary>
        /// Gets all dictionary values that specified property can have.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>List&lt;PropertyValue&gt;</returns>
        List<PropertyValue> CatalogModulePropertiesGetPropertyValues(string propertyId, string keyword = null);

        /// <summary>
        /// Gets all dictionary values that specified property can have.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>ApiResponse of List&lt;PropertyValue&gt;</returns>
        ApiResponse<List<PropertyValue>> CatalogModulePropertiesGetPropertyValuesWithHttpInfo(string propertyId, string keyword = null);
        /// <summary>
        /// Searches for the items by complex criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>CatalogSearchResult</returns>
        CatalogSearchResult CatalogModuleSearchSearch(SearchCriteria criteria);

        /// <summary>
        /// Searches for the items by complex criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ApiResponse of CatalogSearchResult</returns>
        ApiResponse<CatalogSearchResult> CatalogModuleSearchSearchWithHttpInfo(SearchCriteria criteria);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Creates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Creates the specified catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Task of Catalog</returns>
        System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsCreateAsync(Catalog catalog);

        /// <summary>
        /// Creates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Creates the specified catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsCreateAsyncWithHttpInfo(Catalog catalog);
        /// <summary>
        /// Deletes catalog by id.
        /// </summary>
        /// <remarks>
        /// Deletes catalog by id
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleCatalogsDeleteAsync(string id);

        /// <summary>
        /// Deletes catalog by id.
        /// </summary>
        /// <remarks>
        /// Deletes catalog by id
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCatalogsDeleteAsyncWithHttpInfo(string id);
        /// <summary>
        /// Gets Catalog by id.
        /// </summary>
        /// <remarks>
        /// Gets Catalog by id with full information loaded
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Task of Catalog</returns>
        System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetAsync(string id);

        /// <summary>
        /// Gets Catalog by id.
        /// </summary>
        /// <remarks>
        /// Gets Catalog by id with full information loaded
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get Catalogs list
        /// </summary>
        /// <remarks>
        /// Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Catalog&gt;</returns>
        System.Threading.Tasks.Task<List<Catalog>> CatalogModuleCatalogsGetCatalogsAsync();

        /// <summary>
        /// Get Catalogs list
        /// </summary>
        /// <remarks>
        /// Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Catalog&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Catalog>>> CatalogModuleCatalogsGetCatalogsAsyncWithHttpInfo();
        /// <summary>
        /// Gets the template for a new catalog.
        /// </summary>
        /// <remarks>
        /// Gets the template for a new common catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of Catalog</returns>
        System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetNewCatalogAsync();

        /// <summary>
        /// Gets the template for a new catalog.
        /// </summary>
        /// <remarks>
        /// Gets the template for a new common catalog
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetNewCatalogAsyncWithHttpInfo();
        /// <summary>
        /// Gets the template for a new virtual catalog.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of Catalog</returns>
        System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetNewVirtualCatalogAsync();

        /// <summary>
        /// Gets the template for a new virtual catalog.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetNewVirtualCatalogAsyncWithHttpInfo();
        /// <summary>
        /// Updates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Updates the specified catalog.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleCatalogsUpdateAsync(Catalog catalog);

        /// <summary>
        /// Updates the specified catalog.
        /// </summary>
        /// <remarks>
        /// Updates the specified catalog.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCatalogsUpdateAsyncWithHttpInfo(Catalog catalog);
        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>
        /// If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleCategoriesCreateOrUpdateCategoryAsync(Category category);

        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>
        /// If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCategoriesCreateOrUpdateCategoryAsyncWithHttpInfo(Category category);
        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleCategoriesDeleteAsync(List<string> ids);

        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCategoriesDeleteAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Task of Category</returns>
        System.Threading.Tasks.Task<Category> CatalogModuleCategoriesGetAsync(string id);

        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Task of ApiResponse (Category)</returns>
        System.Threading.Tasks.Task<ApiResponse<Category>> CatalogModuleCategoriesGetAsyncWithHttpInfo(string id);
        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of List&lt;Category&gt;</returns>
        System.Threading.Tasks.Task<List<Category>> CatalogModuleCategoriesGetCategoriesByIdsAsync(List<string> ids, string respGroup = null);

        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;Category&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Category>>> CatalogModuleCategoriesGetCategoriesByIdsAsyncWithHttpInfo(List<string> ids, string respGroup = null);
        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Task of Category</returns>
        System.Threading.Tasks.Task<Category> CatalogModuleCategoriesGetNewCategoryAsync(string catalogId, string parentCategoryId = null);

        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Task of ApiResponse (Category)</returns>
        System.Threading.Tasks.Task<ApiResponse<Category>> CatalogModuleCategoriesGetNewCategoryAsyncWithHttpInfo(string catalogId, string parentCategoryId = null);
        /// <summary>
        /// Start catalog data export process.
        /// </summary>
        /// <remarks>
        /// Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>Task of ExportNotification</returns>
        System.Threading.Tasks.Task<ExportNotification> CatalogModuleExportImportDoExportAsync(CsvExportInfo exportInfo);

        /// <summary>
        /// Start catalog data export process.
        /// </summary>
        /// <remarks>
        /// Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>Task of ApiResponse (ExportNotification)</returns>
        System.Threading.Tasks.Task<ApiResponse<ExportNotification>> CatalogModuleExportImportDoExportAsyncWithHttpInfo(CsvExportInfo exportInfo);
        /// <summary>
        /// Start catalog data import process.
        /// </summary>
        /// <remarks>
        /// Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>Task of ImportNotification</returns>
        System.Threading.Tasks.Task<ImportNotification> CatalogModuleExportImportDoImportAsync(CsvImportInfo importInfo);

        /// <summary>
        /// Start catalog data import process.
        /// </summary>
        /// <remarks>
        /// Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>Task of ApiResponse (ImportNotification)</returns>
        System.Threading.Tasks.Task<ApiResponse<ImportNotification>> CatalogModuleExportImportDoImportAsyncWithHttpInfo(CsvImportInfo importInfo);
        /// <summary>
        /// Gets the CSV mapping configuration.
        /// </summary>
        /// <remarks>
        /// Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>Task of CsvProductMappingConfiguration</returns>
        System.Threading.Tasks.Task<CsvProductMappingConfiguration> CatalogModuleExportImportGetMappingConfigurationAsync(string fileUrl, string delimiter = null);

        /// <summary>
        /// Gets the CSV mapping configuration.
        /// </summary>
        /// <remarks>
        /// Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>Task of ApiResponse (CsvProductMappingConfiguration)</returns>
        System.Threading.Tasks.Task<ApiResponse<CsvProductMappingConfiguration>> CatalogModuleExportImportGetMappingConfigurationAsyncWithHttpInfo(string fileUrl, string delimiter = null);
        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleListEntryCreateLinksAsync(List<ListEntryLink> links);

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryCreateLinksAsyncWithHttpInfo(List<ListEntryLink> links);
        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleListEntryDeleteLinksAsync(List<ListEntryLink> links);

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryDeleteLinksAsyncWithHttpInfo(List<ListEntryLink> links);
        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ListEntrySearchResult</returns>
        System.Threading.Tasks.Task<ListEntrySearchResult> CatalogModuleListEntryListItemsSearchAsync(SearchCriteria criteria);

        /// <summary>
        /// Searches for the items by complex criteria.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ApiResponse (ListEntrySearchResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<ListEntrySearchResult>> CatalogModuleListEntryListItemsSearchAsyncWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleListEntryMoveAsync(MoveInfo moveInfo);

        /// <summary>
        /// Move categories or products to another location.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryMoveAsyncWithHttpInfo(MoveInfo moveInfo);
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Task of Product</returns>
        System.Threading.Tasks.Task<Product> CatalogModuleProductsCloneProductAsync(string productId);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Task of ApiResponse (Product)</returns>
        System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsCloneProductAsyncWithHttpInfo(string productId);
        /// <summary>
        /// Deletes the specified items by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleProductsDeleteAsync(List<string> ids);

        /// <summary>
        /// Deletes the specified items by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleProductsDeleteAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Gets the template for a new product (outside of category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog directly.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of Product</returns>
        System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewProductByCatalogAsync(string catalogId);

        /// <summary>
        /// Gets the template for a new product (outside of category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog directly.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewProductByCatalogAsyncWithHttpInfo(string catalogId);
        /// <summary>
        /// Gets the template for a new product (inside category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog category.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of Product</returns>
        System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewProductByCatalogAndCategoryAsync(string catalogId, string categoryId);

        /// <summary>
        /// Gets the template for a new product (inside category).
        /// </summary>
        /// <remarks>
        /// Use when need to create item belonging to catalog category.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewProductByCatalogAndCategoryAsyncWithHttpInfo(string catalogId, string categoryId);
        /// <summary>
        /// Gets the template for a new variation.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Task of Product</returns>
        System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewVariationAsync(string productId);

        /// <summary>
        /// Gets the template for a new variation.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewVariationAsyncWithHttpInfo(string productId);
        /// <summary>
        /// Gets product by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of Product</returns>
        System.Threading.Tasks.Task<Product> CatalogModuleProductsGetProductByIdAsync(string id, string respGroup = null);

        /// <summary>
        /// Gets product by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetProductByIdAsyncWithHttpInfo(string id, string respGroup = null);
        /// <summary>
        /// Gets products by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of List&lt;Product&gt;</returns>
        System.Threading.Tasks.Task<List<Product>> CatalogModuleProductsGetProductByIdsAsync(List<string> ids, string respGroup = null);

        /// <summary>
        /// Gets products by ids
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;Product&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Product>>> CatalogModuleProductsGetProductByIdsAsyncWithHttpInfo(List<string> ids, string respGroup = null);
        /// <summary>
        /// Updates the specified product.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModuleProductsUpdateAsync(Product product);

        /// <summary>
        /// Updates the specified product.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleProductsUpdateAsyncWithHttpInfo(Product product);
        /// <summary>
        /// Creates or updates the specified property.
        /// </summary>
        /// <remarks>
        /// If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModulePropertiesCreateOrUpdatePropertyAsync(Property property);

        /// <summary>
        /// Creates or updates the specified property.
        /// </summary>
        /// <remarks>
        /// If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModulePropertiesCreateOrUpdatePropertyAsyncWithHttpInfo(Property property);
        /// <summary>
        /// Deletes property by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CatalogModulePropertiesDeleteAsync(string id);

        /// <summary>
        /// Deletes property by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CatalogModulePropertiesDeleteAsyncWithHttpInfo(string id);
        /// <summary>
        /// Gets property metainformation by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Task of Property</returns>
        System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetAsync(string propertyId);

        /// <summary>
        /// Gets property metainformation by id.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetAsyncWithHttpInfo(string propertyId);
        /// <summary>
        /// Gets the template for a new catalog property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of Property</returns>
        System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetNewCatalogPropertyAsync(string catalogId);

        /// <summary>
        /// Gets the template for a new catalog property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetNewCatalogPropertyAsyncWithHttpInfo(string catalogId);
        /// <summary>
        /// Gets the template for a new category property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of Property</returns>
        System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetNewCategoryPropertyAsync(string categoryId);

        /// <summary>
        /// Gets the template for a new category property.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetNewCategoryPropertyAsyncWithHttpInfo(string categoryId);
        /// <summary>
        /// Gets all dictionary values that specified property can have.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>Task of List&lt;PropertyValue&gt;</returns>
        System.Threading.Tasks.Task<List<PropertyValue>> CatalogModulePropertiesGetPropertyValuesAsync(string propertyId, string keyword = null);

        /// <summary>
        /// Gets all dictionary values that specified property can have.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;PropertyValue&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<PropertyValue>>> CatalogModulePropertiesGetPropertyValuesAsyncWithHttpInfo(string propertyId, string keyword = null);
        /// <summary>
        /// Searches for the items by complex criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of CatalogSearchResult</returns>
        System.Threading.Tasks.Task<CatalogSearchResult> CatalogModuleSearchSearchAsync(SearchCriteria criteria);

        /// <summary>
        /// Searches for the items by complex criteria
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ApiResponse (CatalogSearchResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<CatalogSearchResult>> CatalogModuleSearchSearchAsyncWithHttpInfo(SearchCriteria criteria);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class VirtoCommerceCatalogApi : IVirtoCommerceCatalogApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommerceCatalogApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="apiClient">An instance of ApiClient.</param>
        /// <returns></returns>
        public VirtoCommerceCatalogApi(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Configuration = apiClient.Configuration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the API client object
        /// </summary>
        /// <value>An instance of the ApiClient</value>
        public ApiClient ApiClient { get; set; }

        /// <summary>
        /// Creates the specified catalog. Creates the specified catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Catalog</returns>
        public Catalog CatalogModuleCatalogsCreate(Catalog catalog)
        {
             ApiResponse<Catalog> localVarResponse = CatalogModuleCatalogsCreateWithHttpInfo(catalog);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Creates the specified catalog. Creates the specified catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>ApiResponse of Catalog</returns>
        public ApiResponse<Catalog> CatalogModuleCatalogsCreateWithHttpInfo(Catalog catalog)
        {
            // verify the required parameter 'catalog' is set
            if (catalog == null)
                throw new ApiException(400, "Missing required parameter 'catalog' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsCreate");

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalog.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(catalog); // http body (model) parameter
            }
            else
            {
                localVarPostBody = catalog; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsCreate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsCreate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }

        /// <summary>
        /// Creates the specified catalog. Creates the specified catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Task of Catalog</returns>
        public async System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsCreateAsync(Catalog catalog)
        {
             ApiResponse<Catalog> localVarResponse = await CatalogModuleCatalogsCreateAsyncWithHttpInfo(catalog);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Creates the specified catalog. Creates the specified catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog to create</param>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsCreateAsyncWithHttpInfo(Catalog catalog)
        {
            // verify the required parameter 'catalog' is set
            if (catalog == null)
                throw new ApiException(400, "Missing required parameter 'catalog' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsCreate");

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalog.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(catalog); // http body (model) parameter
            }
            else
            {
                localVarPostBody = catalog; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsCreate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsCreate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }
        /// <summary>
        /// Deletes catalog by id. Deletes catalog by id
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns></returns>
        public void CatalogModuleCatalogsDelete(string id)
        {
             CatalogModuleCatalogsDeleteWithHttpInfo(id);
        }

        /// <summary>
        /// Deletes catalog by id. Deletes catalog by id
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleCatalogsDeleteWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsDelete");

            var localVarPath = "/api/catalog/catalogs/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Deletes catalog by id. Deletes catalog by id
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleCatalogsDeleteAsync(string id)
        {
             await CatalogModuleCatalogsDeleteAsyncWithHttpInfo(id);

        }

        /// <summary>
        /// Deletes catalog by id. Deletes catalog by id
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Catalog id.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCatalogsDeleteAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsDelete");

            var localVarPath = "/api/catalog/catalogs/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Gets Catalog by id. Gets Catalog by id with full information loaded
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Catalog</returns>
        public Catalog CatalogModuleCatalogsGet(string id)
        {
             ApiResponse<Catalog> localVarResponse = CatalogModuleCatalogsGetWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets Catalog by id. Gets Catalog by id with full information loaded
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>ApiResponse of Catalog</returns>
        public ApiResponse<Catalog> CatalogModuleCatalogsGetWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsGet");

            var localVarPath = "/api/catalog/catalogs/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }

        /// <summary>
        /// Gets Catalog by id. Gets Catalog by id with full information loaded
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Task of Catalog</returns>
        public async System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetAsync(string id)
        {
             ApiResponse<Catalog> localVarResponse = await CatalogModuleCatalogsGetAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets Catalog by id. Gets Catalog by id with full information loaded
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The Catalog id.</param>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsGet");

            var localVarPath = "/api/catalog/catalogs/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }
        /// <summary>
        /// Get Catalogs list Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Catalog&gt;</returns>
        public List<Catalog> CatalogModuleCatalogsGetCatalogs()
        {
             ApiResponse<List<Catalog>> localVarResponse = CatalogModuleCatalogsGetCatalogsWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get Catalogs list Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Catalog&gt;</returns>
        public ApiResponse<List<Catalog>> CatalogModuleCatalogsGetCatalogsWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetCatalogs: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetCatalogs: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Catalog>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Catalog>)ApiClient.Deserialize(localVarResponse, typeof(List<Catalog>)));
            
        }

        /// <summary>
        /// Get Catalogs list Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Catalog&gt;</returns>
        public async System.Threading.Tasks.Task<List<Catalog>> CatalogModuleCatalogsGetCatalogsAsync()
        {
             ApiResponse<List<Catalog>> localVarResponse = await CatalogModuleCatalogsGetCatalogsAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get Catalogs list Get common and virtual Catalogs list with minimal information included. Returns array of Catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Catalog&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Catalog>>> CatalogModuleCatalogsGetCatalogsAsyncWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetCatalogs: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetCatalogs: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Catalog>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Catalog>)ApiClient.Deserialize(localVarResponse, typeof(List<Catalog>)));
            
        }
        /// <summary>
        /// Gets the template for a new catalog. Gets the template for a new common catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Catalog</returns>
        public Catalog CatalogModuleCatalogsGetNewCatalog()
        {
             ApiResponse<Catalog> localVarResponse = CatalogModuleCatalogsGetNewCatalogWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new catalog. Gets the template for a new common catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Catalog</returns>
        public ApiResponse<Catalog> CatalogModuleCatalogsGetNewCatalogWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }

        /// <summary>
        /// Gets the template for a new catalog. Gets the template for a new common catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of Catalog</returns>
        public async System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetNewCatalogAsync()
        {
             ApiResponse<Catalog> localVarResponse = await CatalogModuleCatalogsGetNewCatalogAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new catalog. Gets the template for a new common catalog
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetNewCatalogAsyncWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }
        /// <summary>
        /// Gets the template for a new virtual catalog. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Catalog</returns>
        public Catalog CatalogModuleCatalogsGetNewVirtualCatalog()
        {
             ApiResponse<Catalog> localVarResponse = CatalogModuleCatalogsGetNewVirtualCatalogWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new virtual catalog. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Catalog</returns>
        public ApiResponse<Catalog> CatalogModuleCatalogsGetNewVirtualCatalogWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs/getnewvirtual";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewVirtualCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewVirtualCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }

        /// <summary>
        /// Gets the template for a new virtual catalog. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of Catalog</returns>
        public async System.Threading.Tasks.Task<Catalog> CatalogModuleCatalogsGetNewVirtualCatalogAsync()
        {
             ApiResponse<Catalog> localVarResponse = await CatalogModuleCatalogsGetNewVirtualCatalogAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new virtual catalog. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (Catalog)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Catalog>> CatalogModuleCatalogsGetNewVirtualCatalogAsyncWithHttpInfo()
        {

            var localVarPath = "/api/catalog/catalogs/getnewvirtual";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewVirtualCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsGetNewVirtualCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Catalog>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Catalog)ApiClient.Deserialize(localVarResponse, typeof(Catalog)));
            
        }
        /// <summary>
        /// Updates the specified catalog. Updates the specified catalog.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns></returns>
        public void CatalogModuleCatalogsUpdate(Catalog catalog)
        {
             CatalogModuleCatalogsUpdateWithHttpInfo(catalog);
        }

        /// <summary>
        /// Updates the specified catalog. Updates the specified catalog.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleCatalogsUpdateWithHttpInfo(Catalog catalog)
        {
            // verify the required parameter 'catalog' is set
            if (catalog == null)
                throw new ApiException(400, "Missing required parameter 'catalog' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsUpdate");

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalog.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(catalog); // http body (model) parameter
            }
            else
            {
                localVarPostBody = catalog; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Updates the specified catalog. Updates the specified catalog.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleCatalogsUpdateAsync(Catalog catalog)
        {
             await CatalogModuleCatalogsUpdateAsyncWithHttpInfo(catalog);

        }

        /// <summary>
        /// Updates the specified catalog. Updates the specified catalog.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalog">The catalog.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCatalogsUpdateAsyncWithHttpInfo(Catalog catalog)
        {
            // verify the required parameter 'catalog' is set
            if (catalog == null)
                throw new ApiException(400, "Missing required parameter 'catalog' when calling VirtoCommerceCatalogApi->CatalogModuleCatalogsUpdate");

            var localVarPath = "/api/catalog/catalogs";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalog.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(catalog); // http body (model) parameter
            }
            else
            {
                localVarPostBody = catalog; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCatalogsUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Creates or updates the specified category. If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public void CatalogModuleCategoriesCreateOrUpdateCategory(Category category)
        {
             CatalogModuleCategoriesCreateOrUpdateCategoryWithHttpInfo(category);
        }

        /// <summary>
        /// Creates or updates the specified category. If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleCategoriesCreateOrUpdateCategoryWithHttpInfo(Category category)
        {
            // verify the required parameter 'category' is set
            if (category == null)
                throw new ApiException(400, "Missing required parameter 'category' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesCreateOrUpdateCategory");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (category.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(category); // http body (model) parameter
            }
            else
            {
                localVarPostBody = category; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesCreateOrUpdateCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesCreateOrUpdateCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Creates or updates the specified category. If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleCategoriesCreateOrUpdateCategoryAsync(Category category)
        {
             await CatalogModuleCategoriesCreateOrUpdateCategoryAsyncWithHttpInfo(category);

        }

        /// <summary>
        /// Creates or updates the specified category. If category.id is null, a new category is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="category">The category.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCategoriesCreateOrUpdateCategoryAsyncWithHttpInfo(Category category)
        {
            // verify the required parameter 'category' is set
            if (category == null)
                throw new ApiException(400, "Missing required parameter 'category' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesCreateOrUpdateCategory");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (category.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(category); // http body (model) parameter
            }
            else
            {
                localVarPostBody = category; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesCreateOrUpdateCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesCreateOrUpdateCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Deletes the specified categories by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns></returns>
        public void CatalogModuleCategoriesDelete(List<string> ids)
        {
             CatalogModuleCategoriesDeleteWithHttpInfo(ids);
        }

        /// <summary>
        /// Deletes the specified categories by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleCategoriesDeleteWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesDelete");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Deletes the specified categories by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleCategoriesDeleteAsync(List<string> ids)
        {
             await CatalogModuleCategoriesDeleteAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Deletes the specified categories by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The categories ids.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleCategoriesDeleteAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesDelete");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Gets category by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Category</returns>
        public Category CatalogModuleCategoriesGet(string id)
        {
             ApiResponse<Category> localVarResponse = CatalogModuleCategoriesGetWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets category by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>ApiResponse of Category</returns>
        public ApiResponse<Category> CatalogModuleCategoriesGetWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGet");

            var localVarPath = "/api/catalog/categories/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Category>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Category)ApiClient.Deserialize(localVarResponse, typeof(Category)));
            
        }

        /// <summary>
        /// Gets category by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Task of Category</returns>
        public async System.Threading.Tasks.Task<Category> CatalogModuleCategoriesGetAsync(string id)
        {
             ApiResponse<Category> localVarResponse = await CatalogModuleCategoriesGetAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets category by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Category id.</param>
        /// <returns>Task of ApiResponse (Category)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Category>> CatalogModuleCategoriesGetAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGet");

            var localVarPath = "/api/catalog/categories/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Category>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Category)ApiClient.Deserialize(localVarResponse, typeof(Category)));
            
        }
        /// <summary>
        /// Gets categories by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>List&lt;Category&gt;</returns>
        public List<Category> CatalogModuleCategoriesGetCategoriesByIds(List<string> ids, string respGroup = null)
        {
             ApiResponse<List<Category>> localVarResponse = CatalogModuleCategoriesGetCategoriesByIdsWithHttpInfo(ids, respGroup);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets categories by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of List&lt;Category&gt;</returns>
        public ApiResponse<List<Category>> CatalogModuleCategoriesGetCategoriesByIdsWithHttpInfo(List<string> ids, string respGroup = null)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGetCategoriesByIds");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetCategoriesByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetCategoriesByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Category>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Category>)ApiClient.Deserialize(localVarResponse, typeof(List<Category>)));
            
        }

        /// <summary>
        /// Gets categories by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of List&lt;Category&gt;</returns>
        public async System.Threading.Tasks.Task<List<Category>> CatalogModuleCategoriesGetCategoriesByIdsAsync(List<string> ids, string respGroup = null)
        {
             ApiResponse<List<Category>> localVarResponse = await CatalogModuleCategoriesGetCategoriesByIdsAsyncWithHttpInfo(ids, respGroup);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets categories by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;Category&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Category>>> CatalogModuleCategoriesGetCategoriesByIdsAsyncWithHttpInfo(List<string> ids, string respGroup = null)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGetCategoriesByIds");

            var localVarPath = "/api/catalog/categories";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetCategoriesByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetCategoriesByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Category>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Category>)ApiClient.Deserialize(localVarResponse, typeof(List<Category>)));
            
        }
        /// <summary>
        /// Gets the template for a new category. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Category</returns>
        public Category CatalogModuleCategoriesGetNewCategory(string catalogId, string parentCategoryId = null)
        {
             ApiResponse<Category> localVarResponse = CatalogModuleCategoriesGetNewCategoryWithHttpInfo(catalogId, parentCategoryId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new category. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>ApiResponse of Category</returns>
        public ApiResponse<Category> CatalogModuleCategoriesGetNewCategoryWithHttpInfo(string catalogId, string parentCategoryId = null)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGetNewCategory");

            var localVarPath = "/api/catalog/{catalogId}/categories/newcategory";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter
            if (parentCategoryId != null) localVarQueryParams.Add("parentCategoryId", ApiClient.ParameterToString(parentCategoryId)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetNewCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetNewCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Category>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Category)ApiClient.Deserialize(localVarResponse, typeof(Category)));
            
        }

        /// <summary>
        /// Gets the template for a new category. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Task of Category</returns>
        public async System.Threading.Tasks.Task<Category> CatalogModuleCategoriesGetNewCategoryAsync(string catalogId, string parentCategoryId = null)
        {
             ApiResponse<Category> localVarResponse = await CatalogModuleCategoriesGetNewCategoryAsyncWithHttpInfo(catalogId, parentCategoryId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new category. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional) (optional)</param>
        /// <returns>Task of ApiResponse (Category)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Category>> CatalogModuleCategoriesGetNewCategoryAsyncWithHttpInfo(string catalogId, string parentCategoryId = null)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleCategoriesGetNewCategory");

            var localVarPath = "/api/catalog/{catalogId}/categories/newcategory";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter
            if (parentCategoryId != null) localVarQueryParams.Add("parentCategoryId", ApiClient.ParameterToString(parentCategoryId)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetNewCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleCategoriesGetNewCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Category>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Category)ApiClient.Deserialize(localVarResponse, typeof(Category)));
            
        }
        /// <summary>
        /// Start catalog data export process. Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>ExportNotification</returns>
        public ExportNotification CatalogModuleExportImportDoExport(CsvExportInfo exportInfo)
        {
             ApiResponse<ExportNotification> localVarResponse = CatalogModuleExportImportDoExportWithHttpInfo(exportInfo);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Start catalog data export process. Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>ApiResponse of ExportNotification</returns>
        public ApiResponse<ExportNotification> CatalogModuleExportImportDoExportWithHttpInfo(CsvExportInfo exportInfo)
        {
            // verify the required parameter 'exportInfo' is set
            if (exportInfo == null)
                throw new ApiException(400, "Missing required parameter 'exportInfo' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportDoExport");

            var localVarPath = "/api/catalog/export";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (exportInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(exportInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = exportInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoExport: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoExport: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ExportNotification>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ExportNotification)ApiClient.Deserialize(localVarResponse, typeof(ExportNotification)));
            
        }

        /// <summary>
        /// Start catalog data export process. Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>Task of ExportNotification</returns>
        public async System.Threading.Tasks.Task<ExportNotification> CatalogModuleExportImportDoExportAsync(CsvExportInfo exportInfo)
        {
             ApiResponse<ExportNotification> localVarResponse = await CatalogModuleExportImportDoExportAsyncWithHttpInfo(exportInfo);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Start catalog data export process. Data export is an async process. An ExportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="exportInfo">The export configuration.</param>
        /// <returns>Task of ApiResponse (ExportNotification)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<ExportNotification>> CatalogModuleExportImportDoExportAsyncWithHttpInfo(CsvExportInfo exportInfo)
        {
            // verify the required parameter 'exportInfo' is set
            if (exportInfo == null)
                throw new ApiException(400, "Missing required parameter 'exportInfo' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportDoExport");

            var localVarPath = "/api/catalog/export";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (exportInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(exportInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = exportInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoExport: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoExport: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ExportNotification>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ExportNotification)ApiClient.Deserialize(localVarResponse, typeof(ExportNotification)));
            
        }
        /// <summary>
        /// Start catalog data import process. Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>ImportNotification</returns>
        public ImportNotification CatalogModuleExportImportDoImport(CsvImportInfo importInfo)
        {
             ApiResponse<ImportNotification> localVarResponse = CatalogModuleExportImportDoImportWithHttpInfo(importInfo);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Start catalog data import process. Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>ApiResponse of ImportNotification</returns>
        public ApiResponse<ImportNotification> CatalogModuleExportImportDoImportWithHttpInfo(CsvImportInfo importInfo)
        {
            // verify the required parameter 'importInfo' is set
            if (importInfo == null)
                throw new ApiException(400, "Missing required parameter 'importInfo' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportDoImport");

            var localVarPath = "/api/catalog/import";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (importInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(importInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = importInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoImport: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoImport: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ImportNotification>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ImportNotification)ApiClient.Deserialize(localVarResponse, typeof(ImportNotification)));
            
        }

        /// <summary>
        /// Start catalog data import process. Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>Task of ImportNotification</returns>
        public async System.Threading.Tasks.Task<ImportNotification> CatalogModuleExportImportDoImportAsync(CsvImportInfo importInfo)
        {
             ApiResponse<ImportNotification> localVarResponse = await CatalogModuleExportImportDoImportAsyncWithHttpInfo(importInfo);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Start catalog data import process. Data import is an async process. An ImportNotification is returned for progress reporting.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="importInfo">The import data configuration.</param>
        /// <returns>Task of ApiResponse (ImportNotification)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<ImportNotification>> CatalogModuleExportImportDoImportAsyncWithHttpInfo(CsvImportInfo importInfo)
        {
            // verify the required parameter 'importInfo' is set
            if (importInfo == null)
                throw new ApiException(400, "Missing required parameter 'importInfo' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportDoImport");

            var localVarPath = "/api/catalog/import";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (importInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(importInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = importInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoImport: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportDoImport: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ImportNotification>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ImportNotification)ApiClient.Deserialize(localVarResponse, typeof(ImportNotification)));
            
        }
        /// <summary>
        /// Gets the CSV mapping configuration. Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>CsvProductMappingConfiguration</returns>
        public CsvProductMappingConfiguration CatalogModuleExportImportGetMappingConfiguration(string fileUrl, string delimiter = null)
        {
             ApiResponse<CsvProductMappingConfiguration> localVarResponse = CatalogModuleExportImportGetMappingConfigurationWithHttpInfo(fileUrl, delimiter);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the CSV mapping configuration. Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>ApiResponse of CsvProductMappingConfiguration</returns>
        public ApiResponse<CsvProductMappingConfiguration> CatalogModuleExportImportGetMappingConfigurationWithHttpInfo(string fileUrl, string delimiter = null)
        {
            // verify the required parameter 'fileUrl' is set
            if (fileUrl == null)
                throw new ApiException(400, "Missing required parameter 'fileUrl' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportGetMappingConfiguration");

            var localVarPath = "/api/catalog/import/mappingconfiguration";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (fileUrl != null) localVarQueryParams.Add("fileUrl", ApiClient.ParameterToString(fileUrl)); // query parameter
            if (delimiter != null) localVarQueryParams.Add("delimiter", ApiClient.ParameterToString(delimiter)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportGetMappingConfiguration: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportGetMappingConfiguration: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CsvProductMappingConfiguration>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CsvProductMappingConfiguration)ApiClient.Deserialize(localVarResponse, typeof(CsvProductMappingConfiguration)));
            
        }

        /// <summary>
        /// Gets the CSV mapping configuration. Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>Task of CsvProductMappingConfiguration</returns>
        public async System.Threading.Tasks.Task<CsvProductMappingConfiguration> CatalogModuleExportImportGetMappingConfigurationAsync(string fileUrl, string delimiter = null)
        {
             ApiResponse<CsvProductMappingConfiguration> localVarResponse = await CatalogModuleExportImportGetMappingConfigurationAsyncWithHttpInfo(fileUrl, delimiter);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the CSV mapping configuration. Analyses the supplied file&#39;s structure and returns automatic column mapping.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="delimiter">The CSV delimiter. (optional)</param>
        /// <returns>Task of ApiResponse (CsvProductMappingConfiguration)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CsvProductMappingConfiguration>> CatalogModuleExportImportGetMappingConfigurationAsyncWithHttpInfo(string fileUrl, string delimiter = null)
        {
            // verify the required parameter 'fileUrl' is set
            if (fileUrl == null)
                throw new ApiException(400, "Missing required parameter 'fileUrl' when calling VirtoCommerceCatalogApi->CatalogModuleExportImportGetMappingConfiguration");

            var localVarPath = "/api/catalog/import/mappingconfiguration";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (fileUrl != null) localVarQueryParams.Add("fileUrl", ApiClient.ParameterToString(fileUrl)); // query parameter
            if (delimiter != null) localVarQueryParams.Add("delimiter", ApiClient.ParameterToString(delimiter)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportGetMappingConfiguration: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleExportImportGetMappingConfiguration: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CsvProductMappingConfiguration>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CsvProductMappingConfiguration)ApiClient.Deserialize(localVarResponse, typeof(CsvProductMappingConfiguration)));
            
        }
        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns></returns>
        public void CatalogModuleListEntryCreateLinks(List<ListEntryLink> links)
        {
             CatalogModuleListEntryCreateLinksWithHttpInfo(links);
        }

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleListEntryCreateLinksWithHttpInfo(List<ListEntryLink> links)
        {
            // verify the required parameter 'links' is set
            if (links == null)
                throw new ApiException(400, "Missing required parameter 'links' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryCreateLinks");

            var localVarPath = "/api/catalog/listentrylinks";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (links.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(links); // http body (model) parameter
            }
            else
            {
                localVarPostBody = links; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryCreateLinks: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryCreateLinks: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleListEntryCreateLinksAsync(List<ListEntryLink> links)
        {
             await CatalogModuleListEntryCreateLinksAsyncWithHttpInfo(links);

        }

        /// <summary>
        /// Creates links for categories or items to parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryCreateLinksAsyncWithHttpInfo(List<ListEntryLink> links)
        {
            // verify the required parameter 'links' is set
            if (links == null)
                throw new ApiException(400, "Missing required parameter 'links' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryCreateLinks");

            var localVarPath = "/api/catalog/listentrylinks";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (links.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(links); // http body (model) parameter
            }
            else
            {
                localVarPostBody = links; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryCreateLinks: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryCreateLinks: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns></returns>
        public void CatalogModuleListEntryDeleteLinks(List<ListEntryLink> links)
        {
             CatalogModuleListEntryDeleteLinksWithHttpInfo(links);
        }

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleListEntryDeleteLinksWithHttpInfo(List<ListEntryLink> links)
        {
            // verify the required parameter 'links' is set
            if (links == null)
                throw new ApiException(400, "Missing required parameter 'links' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryDeleteLinks");

            var localVarPath = "/api/catalog/listentrylinks/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (links.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(links); // http body (model) parameter
            }
            else
            {
                localVarPostBody = links; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryDeleteLinks: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryDeleteLinks: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleListEntryDeleteLinksAsync(List<ListEntryLink> links)
        {
             await CatalogModuleListEntryDeleteLinksAsyncWithHttpInfo(links);

        }

        /// <summary>
        /// Unlinks the linked categories or items from parent categories and catalogs. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="links">The links.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryDeleteLinksAsyncWithHttpInfo(List<ListEntryLink> links)
        {
            // verify the required parameter 'links' is set
            if (links == null)
                throw new ApiException(400, "Missing required parameter 'links' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryDeleteLinks");

            var localVarPath = "/api/catalog/listentrylinks/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (links.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(links); // http body (model) parameter
            }
            else
            {
                localVarPostBody = links; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryDeleteLinks: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryDeleteLinks: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Searches for the items by complex criteria. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ListEntrySearchResult</returns>
        public ListEntrySearchResult CatalogModuleListEntryListItemsSearch(SearchCriteria criteria)
        {
             ApiResponse<ListEntrySearchResult> localVarResponse = CatalogModuleListEntryListItemsSearchWithHttpInfo(criteria);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Searches for the items by complex criteria. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ApiResponse of ListEntrySearchResult</returns>
        public ApiResponse<ListEntrySearchResult> CatalogModuleListEntryListItemsSearchWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryListItemsSearch");

            var localVarPath = "/api/catalog/listentries";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryListItemsSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryListItemsSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ListEntrySearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ListEntrySearchResult)ApiClient.Deserialize(localVarResponse, typeof(ListEntrySearchResult)));
            
        }

        /// <summary>
        /// Searches for the items by complex criteria. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ListEntrySearchResult</returns>
        public async System.Threading.Tasks.Task<ListEntrySearchResult> CatalogModuleListEntryListItemsSearchAsync(SearchCriteria criteria)
        {
             ApiResponse<ListEntrySearchResult> localVarResponse = await CatalogModuleListEntryListItemsSearchAsyncWithHttpInfo(criteria);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Searches for the items by complex criteria. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ApiResponse (ListEntrySearchResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<ListEntrySearchResult>> CatalogModuleListEntryListItemsSearchAsyncWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryListItemsSearch");

            var localVarPath = "/api/catalog/listentries";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryListItemsSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryListItemsSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<ListEntrySearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ListEntrySearchResult)ApiClient.Deserialize(localVarResponse, typeof(ListEntrySearchResult)));
            
        }
        /// <summary>
        /// Move categories or products to another location. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns></returns>
        public void CatalogModuleListEntryMove(MoveInfo moveInfo)
        {
             CatalogModuleListEntryMoveWithHttpInfo(moveInfo);
        }

        /// <summary>
        /// Move categories or products to another location. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleListEntryMoveWithHttpInfo(MoveInfo moveInfo)
        {
            // verify the required parameter 'moveInfo' is set
            if (moveInfo == null)
                throw new ApiException(400, "Missing required parameter 'moveInfo' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryMove");

            var localVarPath = "/api/catalog/listentries/move";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (moveInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(moveInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = moveInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryMove: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryMove: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Move categories or products to another location. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleListEntryMoveAsync(MoveInfo moveInfo)
        {
             await CatalogModuleListEntryMoveAsyncWithHttpInfo(moveInfo);

        }

        /// <summary>
        /// Move categories or products to another location. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="moveInfo">Move operation details</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleListEntryMoveAsyncWithHttpInfo(MoveInfo moveInfo)
        {
            // verify the required parameter 'moveInfo' is set
            if (moveInfo == null)
                throw new ApiException(400, "Missing required parameter 'moveInfo' when calling VirtoCommerceCatalogApi->CatalogModuleListEntryMove");

            var localVarPath = "/api/catalog/listentries/move";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (moveInfo.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(moveInfo); // http body (model) parameter
            }
            else
            {
                localVarPostBody = moveInfo; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryMove: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleListEntryMove: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Product</returns>
        public Product CatalogModuleProductsCloneProduct(string productId)
        {
             ApiResponse<Product> localVarResponse = CatalogModuleProductsCloneProductWithHttpInfo(productId);
             return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>ApiResponse of Product</returns>
        public ApiResponse<Product> CatalogModuleProductsCloneProductWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsCloneProduct");

            var localVarPath = "/api/catalog/products/{productId}/clone";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsCloneProduct: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsCloneProduct: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Task of Product</returns>
        public async System.Threading.Tasks.Task<Product> CatalogModuleProductsCloneProductAsync(string productId)
        {
             ApiResponse<Product> localVarResponse = await CatalogModuleProductsCloneProductAsyncWithHttpInfo(productId);
             return localVarResponse.Data;

        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId"></param>
        /// <returns>Task of ApiResponse (Product)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsCloneProductAsyncWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsCloneProduct");

            var localVarPath = "/api/catalog/products/{productId}/clone";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsCloneProduct: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsCloneProduct: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }
        /// <summary>
        /// Deletes the specified items by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns></returns>
        public void CatalogModuleProductsDelete(List<string> ids)
        {
             CatalogModuleProductsDeleteWithHttpInfo(ids);
        }

        /// <summary>
        /// Deletes the specified items by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleProductsDeleteWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleProductsDelete");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Deletes the specified items by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleProductsDeleteAsync(List<string> ids)
        {
             await CatalogModuleProductsDeleteAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Deletes the specified items by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">The items ids.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleProductsDeleteAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleProductsDelete");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Gets the template for a new product (outside of category). Use when need to create item belonging to catalog directly.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Product</returns>
        public Product CatalogModuleProductsGetNewProductByCatalog(string catalogId)
        {
             ApiResponse<Product> localVarResponse = CatalogModuleProductsGetNewProductByCatalogWithHttpInfo(catalogId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new product (outside of category). Use when need to create item belonging to catalog directly.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>ApiResponse of Product</returns>
        public ApiResponse<Product> CatalogModuleProductsGetNewProductByCatalogWithHttpInfo(string catalogId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalog");

            var localVarPath = "/api/catalog/{catalogId}/products/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }

        /// <summary>
        /// Gets the template for a new product (outside of category). Use when need to create item belonging to catalog directly.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of Product</returns>
        public async System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewProductByCatalogAsync(string catalogId)
        {
             ApiResponse<Product> localVarResponse = await CatalogModuleProductsGetNewProductByCatalogAsyncWithHttpInfo(catalogId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new product (outside of category). Use when need to create item belonging to catalog directly.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewProductByCatalogAsyncWithHttpInfo(string catalogId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalog");

            var localVarPath = "/api/catalog/{catalogId}/products/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalog: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalog: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }
        /// <summary>
        /// Gets the template for a new product (inside category). Use when need to create item belonging to catalog category.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Product</returns>
        public Product CatalogModuleProductsGetNewProductByCatalogAndCategory(string catalogId, string categoryId)
        {
             ApiResponse<Product> localVarResponse = CatalogModuleProductsGetNewProductByCatalogAndCategoryWithHttpInfo(catalogId, categoryId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new product (inside category). Use when need to create item belonging to catalog category.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>ApiResponse of Product</returns>
        public ApiResponse<Product> CatalogModuleProductsGetNewProductByCatalogAndCategoryWithHttpInfo(string catalogId, string categoryId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalogAndCategory");
            // verify the required parameter 'categoryId' is set
            if (categoryId == null)
                throw new ApiException(400, "Missing required parameter 'categoryId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalogAndCategory");

            var localVarPath = "/api/catalog/{catalogId}/categories/{categoryId}/products/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter
            if (categoryId != null) localVarPathParams.Add("categoryId", ApiClient.ParameterToString(categoryId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalogAndCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalogAndCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }

        /// <summary>
        /// Gets the template for a new product (inside category). Use when need to create item belonging to catalog category.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of Product</returns>
        public async System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewProductByCatalogAndCategoryAsync(string catalogId, string categoryId)
        {
             ApiResponse<Product> localVarResponse = await CatalogModuleProductsGetNewProductByCatalogAndCategoryAsyncWithHttpInfo(catalogId, categoryId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new product (inside category). Use when need to create item belonging to catalog category.
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewProductByCatalogAndCategoryAsyncWithHttpInfo(string catalogId, string categoryId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalogAndCategory");
            // verify the required parameter 'categoryId' is set
            if (categoryId == null)
                throw new ApiException(400, "Missing required parameter 'categoryId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewProductByCatalogAndCategory");

            var localVarPath = "/api/catalog/{catalogId}/categories/{categoryId}/products/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter
            if (categoryId != null) localVarPathParams.Add("categoryId", ApiClient.ParameterToString(categoryId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalogAndCategory: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewProductByCatalogAndCategory: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }
        /// <summary>
        /// Gets the template for a new variation. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Product</returns>
        public Product CatalogModuleProductsGetNewVariation(string productId)
        {
             ApiResponse<Product> localVarResponse = CatalogModuleProductsGetNewVariationWithHttpInfo(productId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new variation. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>ApiResponse of Product</returns>
        public ApiResponse<Product> CatalogModuleProductsGetNewVariationWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewVariation");

            var localVarPath = "/api/catalog/products/{productId}/getnewvariation";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewVariation: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewVariation: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }

        /// <summary>
        /// Gets the template for a new variation. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Task of Product</returns>
        public async System.Threading.Tasks.Task<Product> CatalogModuleProductsGetNewVariationAsync(string productId)
        {
             ApiResponse<Product> localVarResponse = await CatalogModuleProductsGetNewVariationAsyncWithHttpInfo(productId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new variation. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">The parent product id.</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetNewVariationAsyncWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetNewVariation");

            var localVarPath = "/api/catalog/products/{productId}/getnewvariation";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewVariation: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetNewVariation: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }
        /// <summary>
        /// Gets product by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Product</returns>
        public Product CatalogModuleProductsGetProductById(string id, string respGroup = null)
        {
             ApiResponse<Product> localVarResponse = CatalogModuleProductsGetProductByIdWithHttpInfo(id, respGroup);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets product by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of Product</returns>
        public ApiResponse<Product> CatalogModuleProductsGetProductByIdWithHttpInfo(string id, string respGroup = null)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetProductById");

            var localVarPath = "/api/catalog/products/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }

        /// <summary>
        /// Gets product by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of Product</returns>
        public async System.Threading.Tasks.Task<Product> CatalogModuleProductsGetProductByIdAsync(string id, string respGroup = null)
        {
             ApiResponse<Product> localVarResponse = await CatalogModuleProductsGetProductByIdAsyncWithHttpInfo(id, respGroup);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets product by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Item id.</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (Product)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Product>> CatalogModuleProductsGetProductByIdAsyncWithHttpInfo(string id, string respGroup = null)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetProductById");

            var localVarPath = "/api/catalog/products/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Product>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Product)ApiClient.Deserialize(localVarResponse, typeof(Product)));
            
        }
        /// <summary>
        /// Gets products by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>List&lt;Product&gt;</returns>
        public List<Product> CatalogModuleProductsGetProductByIds(List<string> ids, string respGroup = null)
        {
             ApiResponse<List<Product>> localVarResponse = CatalogModuleProductsGetProductByIdsWithHttpInfo(ids, respGroup);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets products by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>ApiResponse of List&lt;Product&gt;</returns>
        public ApiResponse<List<Product>> CatalogModuleProductsGetProductByIdsWithHttpInfo(List<string> ids, string respGroup = null)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetProductByIds");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Product>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Product>)ApiClient.Deserialize(localVarResponse, typeof(List<Product>)));
            
        }

        /// <summary>
        /// Gets products by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of List&lt;Product&gt;</returns>
        public async System.Threading.Tasks.Task<List<Product>> CatalogModuleProductsGetProductByIdsAsync(List<string> ids, string respGroup = null)
        {
             ApiResponse<List<Product>> localVarResponse = await CatalogModuleProductsGetProductByIdsAsyncWithHttpInfo(ids, respGroup);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets products by ids 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;Product&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Product>>> CatalogModuleProductsGetProductByIdsAsyncWithHttpInfo(List<string> ids, string respGroup = null)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCatalogApi->CatalogModuleProductsGetProductByIds");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter
            if (respGroup != null) localVarQueryParams.Add("respGroup", ApiClient.ParameterToString(respGroup)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductByIds: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsGetProductByIds: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Product>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Product>)ApiClient.Deserialize(localVarResponse, typeof(List<Product>)));
            
        }
        /// <summary>
        /// Updates the specified product. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns></returns>
        public void CatalogModuleProductsUpdate(Product product)
        {
             CatalogModuleProductsUpdateWithHttpInfo(product);
        }

        /// <summary>
        /// Updates the specified product. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModuleProductsUpdateWithHttpInfo(Product product)
        {
            // verify the required parameter 'product' is set
            if (product == null)
                throw new ApiException(400, "Missing required parameter 'product' when calling VirtoCommerceCatalogApi->CatalogModuleProductsUpdate");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (product.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(product); // http body (model) parameter
            }
            else
            {
                localVarPostBody = product; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Updates the specified product. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModuleProductsUpdateAsync(Product product)
        {
             await CatalogModuleProductsUpdateAsyncWithHttpInfo(product);

        }

        /// <summary>
        /// Updates the specified product. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="product">The product.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModuleProductsUpdateAsyncWithHttpInfo(Product product)
        {
            // verify the required parameter 'product' is set
            if (product == null)
                throw new ApiException(400, "Missing required parameter 'product' when calling VirtoCommerceCatalogApi->CatalogModuleProductsUpdate");

            var localVarPath = "/api/catalog/products";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (product.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(product); // http body (model) parameter
            }
            else
            {
                localVarPostBody = product; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleProductsUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Creates or updates the specified property. If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public void CatalogModulePropertiesCreateOrUpdateProperty(Property property)
        {
             CatalogModulePropertiesCreateOrUpdatePropertyWithHttpInfo(property);
        }

        /// <summary>
        /// Creates or updates the specified property. If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModulePropertiesCreateOrUpdatePropertyWithHttpInfo(Property property)
        {
            // verify the required parameter 'property' is set
            if (property == null)
                throw new ApiException(400, "Missing required parameter 'property' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesCreateOrUpdateProperty");

            var localVarPath = "/api/catalog/properties";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (property.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(property); // http body (model) parameter
            }
            else
            {
                localVarPostBody = property; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesCreateOrUpdateProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesCreateOrUpdateProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Creates or updates the specified property. If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModulePropertiesCreateOrUpdatePropertyAsync(Property property)
        {
             await CatalogModulePropertiesCreateOrUpdatePropertyAsyncWithHttpInfo(property);

        }

        /// <summary>
        /// Creates or updates the specified property. If property.IsNew &#x3D;&#x3D; True, a new property is created. It&#39;s updated otherwise
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="property">The property.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModulePropertiesCreateOrUpdatePropertyAsyncWithHttpInfo(Property property)
        {
            // verify the required parameter 'property' is set
            if (property == null)
                throw new ApiException(400, "Missing required parameter 'property' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesCreateOrUpdateProperty");

            var localVarPath = "/api/catalog/properties";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (property.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(property); // http body (model) parameter
            }
            else
            {
                localVarPostBody = property; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesCreateOrUpdateProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesCreateOrUpdateProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Deletes property by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns></returns>
        public void CatalogModulePropertiesDelete(string id)
        {
             CatalogModulePropertiesDeleteWithHttpInfo(id);
        }

        /// <summary>
        /// Deletes property by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CatalogModulePropertiesDeleteWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesDelete");

            var localVarPath = "/api/catalog/properties";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarQueryParams.Add("id", ApiClient.ParameterToString(id)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Deletes property by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CatalogModulePropertiesDeleteAsync(string id)
        {
             await CatalogModulePropertiesDeleteAsyncWithHttpInfo(id);

        }

        /// <summary>
        /// Deletes property by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The property id.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CatalogModulePropertiesDeleteAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesDelete");

            var localVarPath = "/api/catalog/properties";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarQueryParams.Add("id", ApiClient.ParameterToString(id)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Gets property metainformation by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Property</returns>
        public Property CatalogModulePropertiesGet(string propertyId)
        {
             ApiResponse<Property> localVarResponse = CatalogModulePropertiesGetWithHttpInfo(propertyId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets property metainformation by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>ApiResponse of Property</returns>
        public ApiResponse<Property> CatalogModulePropertiesGetWithHttpInfo(string propertyId)
        {
            // verify the required parameter 'propertyId' is set
            if (propertyId == null)
                throw new ApiException(400, "Missing required parameter 'propertyId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGet");

            var localVarPath = "/api/catalog/properties/{propertyId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (propertyId != null) localVarPathParams.Add("propertyId", ApiClient.ParameterToString(propertyId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }

        /// <summary>
        /// Gets property metainformation by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Task of Property</returns>
        public async System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetAsync(string propertyId)
        {
             ApiResponse<Property> localVarResponse = await CatalogModulePropertiesGetAsyncWithHttpInfo(propertyId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets property metainformation by id. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetAsyncWithHttpInfo(string propertyId)
        {
            // verify the required parameter 'propertyId' is set
            if (propertyId == null)
                throw new ApiException(400, "Missing required parameter 'propertyId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGet");

            var localVarPath = "/api/catalog/properties/{propertyId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (propertyId != null) localVarPathParams.Add("propertyId", ApiClient.ParameterToString(propertyId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGet: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGet: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }
        /// <summary>
        /// Gets the template for a new catalog property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Property</returns>
        public Property CatalogModulePropertiesGetNewCatalogProperty(string catalogId)
        {
             ApiResponse<Property> localVarResponse = CatalogModulePropertiesGetNewCatalogPropertyWithHttpInfo(catalogId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new catalog property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>ApiResponse of Property</returns>
        public ApiResponse<Property> CatalogModulePropertiesGetNewCatalogPropertyWithHttpInfo(string catalogId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetNewCatalogProperty");

            var localVarPath = "/api/catalog/{catalogId}/properties/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCatalogProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCatalogProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }

        /// <summary>
        /// Gets the template for a new catalog property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of Property</returns>
        public async System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetNewCatalogPropertyAsync(string catalogId)
        {
             ApiResponse<Property> localVarResponse = await CatalogModulePropertiesGetNewCatalogPropertyAsyncWithHttpInfo(catalogId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new catalog property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="catalogId">The catalog id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetNewCatalogPropertyAsyncWithHttpInfo(string catalogId)
        {
            // verify the required parameter 'catalogId' is set
            if (catalogId == null)
                throw new ApiException(400, "Missing required parameter 'catalogId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetNewCatalogProperty");

            var localVarPath = "/api/catalog/{catalogId}/properties/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (catalogId != null) localVarPathParams.Add("catalogId", ApiClient.ParameterToString(catalogId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCatalogProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCatalogProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }
        /// <summary>
        /// Gets the template for a new category property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Property</returns>
        public Property CatalogModulePropertiesGetNewCategoryProperty(string categoryId)
        {
             ApiResponse<Property> localVarResponse = CatalogModulePropertiesGetNewCategoryPropertyWithHttpInfo(categoryId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets the template for a new category property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>ApiResponse of Property</returns>
        public ApiResponse<Property> CatalogModulePropertiesGetNewCategoryPropertyWithHttpInfo(string categoryId)
        {
            // verify the required parameter 'categoryId' is set
            if (categoryId == null)
                throw new ApiException(400, "Missing required parameter 'categoryId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetNewCategoryProperty");

            var localVarPath = "/api/catalog/categories/{categoryId}/properties/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (categoryId != null) localVarPathParams.Add("categoryId", ApiClient.ParameterToString(categoryId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCategoryProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCategoryProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }

        /// <summary>
        /// Gets the template for a new category property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of Property</returns>
        public async System.Threading.Tasks.Task<Property> CatalogModulePropertiesGetNewCategoryPropertyAsync(string categoryId)
        {
             ApiResponse<Property> localVarResponse = await CatalogModulePropertiesGetNewCategoryPropertyAsyncWithHttpInfo(categoryId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets the template for a new category property. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="categoryId">The category id.</param>
        /// <returns>Task of ApiResponse (Property)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Property>> CatalogModulePropertiesGetNewCategoryPropertyAsyncWithHttpInfo(string categoryId)
        {
            // verify the required parameter 'categoryId' is set
            if (categoryId == null)
                throw new ApiException(400, "Missing required parameter 'categoryId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetNewCategoryProperty");

            var localVarPath = "/api/catalog/categories/{categoryId}/properties/getnew";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (categoryId != null) localVarPathParams.Add("categoryId", ApiClient.ParameterToString(categoryId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCategoryProperty: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetNewCategoryProperty: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Property>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Property)ApiClient.Deserialize(localVarResponse, typeof(Property)));
            
        }
        /// <summary>
        /// Gets all dictionary values that specified property can have. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>List&lt;PropertyValue&gt;</returns>
        public List<PropertyValue> CatalogModulePropertiesGetPropertyValues(string propertyId, string keyword = null)
        {
             ApiResponse<List<PropertyValue>> localVarResponse = CatalogModulePropertiesGetPropertyValuesWithHttpInfo(propertyId, keyword);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Gets all dictionary values that specified property can have. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>ApiResponse of List&lt;PropertyValue&gt;</returns>
        public ApiResponse<List<PropertyValue>> CatalogModulePropertiesGetPropertyValuesWithHttpInfo(string propertyId, string keyword = null)
        {
            // verify the required parameter 'propertyId' is set
            if (propertyId == null)
                throw new ApiException(400, "Missing required parameter 'propertyId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetPropertyValues");

            var localVarPath = "/api/catalog/properties/{propertyId}/values";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (propertyId != null) localVarPathParams.Add("propertyId", ApiClient.ParameterToString(propertyId)); // path parameter
            if (keyword != null) localVarQueryParams.Add("keyword", ApiClient.ParameterToString(keyword)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetPropertyValues: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetPropertyValues: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<PropertyValue>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<PropertyValue>)ApiClient.Deserialize(localVarResponse, typeof(List<PropertyValue>)));
            
        }

        /// <summary>
        /// Gets all dictionary values that specified property can have. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>Task of List&lt;PropertyValue&gt;</returns>
        public async System.Threading.Tasks.Task<List<PropertyValue>> CatalogModulePropertiesGetPropertyValuesAsync(string propertyId, string keyword = null)
        {
             ApiResponse<List<PropertyValue>> localVarResponse = await CatalogModulePropertiesGetPropertyValuesAsyncWithHttpInfo(propertyId, keyword);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Gets all dictionary values that specified property can have. 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional) (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;PropertyValue&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<PropertyValue>>> CatalogModulePropertiesGetPropertyValuesAsyncWithHttpInfo(string propertyId, string keyword = null)
        {
            // verify the required parameter 'propertyId' is set
            if (propertyId == null)
                throw new ApiException(400, "Missing required parameter 'propertyId' when calling VirtoCommerceCatalogApi->CatalogModulePropertiesGetPropertyValues");

            var localVarPath = "/api/catalog/properties/{propertyId}/values";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (propertyId != null) localVarPathParams.Add("propertyId", ApiClient.ParameterToString(propertyId)); // path parameter
            if (keyword != null) localVarQueryParams.Add("keyword", ApiClient.ParameterToString(keyword)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetPropertyValues: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModulePropertiesGetPropertyValues: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<PropertyValue>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<PropertyValue>)ApiClient.Deserialize(localVarResponse, typeof(List<PropertyValue>)));
            
        }
        /// <summary>
        /// Searches for the items by complex criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>CatalogSearchResult</returns>
        public CatalogSearchResult CatalogModuleSearchSearch(SearchCriteria criteria)
        {
             ApiResponse<CatalogSearchResult> localVarResponse = CatalogModuleSearchSearchWithHttpInfo(criteria);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Searches for the items by complex criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>ApiResponse of CatalogSearchResult</returns>
        public ApiResponse<CatalogSearchResult> CatalogModuleSearchSearchWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCatalogApi->CatalogModuleSearchSearch");

            var localVarPath = "/api/catalog/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleSearchSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleSearchSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CatalogSearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CatalogSearchResult)ApiClient.Deserialize(localVarResponse, typeof(CatalogSearchResult)));
            
        }

        /// <summary>
        /// Searches for the items by complex criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of CatalogSearchResult</returns>
        public async System.Threading.Tasks.Task<CatalogSearchResult> CatalogModuleSearchSearchAsync(SearchCriteria criteria)
        {
             ApiResponse<CatalogSearchResult> localVarResponse = await CatalogModuleSearchSearchAsyncWithHttpInfo(criteria);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Searches for the items by complex criteria 
        /// </summary>
        /// <exception cref="VirtoCommerce.CatalogModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Task of ApiResponse (CatalogSearchResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<CatalogSearchResult>> CatalogModuleSearchSearchAsyncWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCatalogApi->CatalogModuleSearchSearch");

            var localVarPath = "/api/catalog/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleSearchSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CatalogModuleSearchSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<CatalogSearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (CatalogSearchResult)ApiClient.Deserialize(localVarResponse, typeof(CatalogSearchResult)));
            
        }
    }
}
