namespace VirtoCommerce.CatalogModule.Web.Model
{
    /// <summary>
    /// Information to search and create links to categories and items
    /// </summary>
    public class BulkCreateLinkParameters
    {
        public SearchCriteria SearchCriteria;

        /// <summary>
        /// Category identifier to which create link
        /// </summary>
        public string CategoryId;

        /// <summary>
        /// Catalog identifier to which create link
        /// </summary>
        public string CatalogId;
    }
}
