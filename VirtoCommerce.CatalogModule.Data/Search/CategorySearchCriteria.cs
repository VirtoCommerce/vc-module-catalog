using System.Collections.Generic;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchCriteria : SearchCriteria
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Category;

        public static SortingField DefaultSortOrder => new SortingField { FieldName = "__sort" };

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
        public virtual IList<string> Outlines { get; set; } = new List<string>();
    }
}
