using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearchCriteria : SearchCriteria
    {
        public virtual string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>
        /// The catalog.
        /// </value>
        public virtual string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the outlines. Outline consists of "Category1/Category2".
        /// </summary>
        /// <example>Everything/digital-cameras</example>
        /// <value>The outlines.</value>
        public virtual string Outline { get; set; }

        /// <summary>
        /// Gets the default sort order.
        /// </summary>
        /// <value>The default sort order.</value>
        public static SortingField DefaultSortOrder => new SortingField { FieldName = "__sort" };
    }
}
