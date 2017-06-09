using System.Collections.Generic;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchCriteria : SearchCriteria
    {
        public const string DocType = Constants.CategoryDocumentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySearchCriteria"/> class.
        /// </summary>
        public CategorySearchCriteria()
            : base(DocType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySearchCriteria"/> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public CategorySearchCriteria(string documentType)
            : base(documentType)
        {
        }

        public static SortingField DefaultSortOrder => new SortingField { FieldName = "__sort" };

        /// <summary>
        /// Gets or sets the outlines. Outline consists of "Category1/Category2".
        /// </summary>
        /// <example>Everything/digital-cameras</example>
        /// <value>The outlines.</value>
        public virtual IList<string> Outlines { get; set; } = new List<string>();
    }
}
