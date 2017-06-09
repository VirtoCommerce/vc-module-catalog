using System;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchCriteria : SearchCriteria
    {
        public const string DocType = Constants.ProductDocumentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSearchCriteria"/> class.
        /// </summary>
        public ProductSearchCriteria()
            : base(DocType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSearchCriteria"/> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public ProductSearchCriteria(string documentType)
            : base(documentType)
        {
        }

        public virtual string SearchPhrase { get; set; }
        public virtual string Locale { get; set; }
        public virtual string Currency { get; set; }
        public virtual IList<string> Pricelists { get; set; }

        public virtual IList<ISearchFilter> CurrentFilters { get; } = new List<ISearchFilter>();

        public virtual IList<ISearchFilter> Filters { get; } = new List<ISearchFilter>();

        /// <summary>
        /// Gets the default sort order.
        /// </summary>
        /// <value>The default sort order.</value>
        public static SortingField DefaultSortOrder => new SortingField { FieldName = "__sort" };

        /// <summary>
        /// Gets or sets the class types.
        /// </summary>
        /// <value>The class types.</value>
        public virtual IList<string> ClassTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the indexes of the search.
        /// </summary>
        /// <value>
        /// The index of the search.
        /// </value>
        public virtual string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the outlines. Outline consists of "Category1/Category2".
        /// </summary>
        /// <example>Everything/digital-cameras</example>
        /// <value>The outlines.</value>
        public virtual IList<string> Outlines { get; set; } = new List<string>();

        /// <summary>
        /// Specifies if we search hidden products.
        /// </summary>
        public virtual bool WithHidden { get; set; }

        /// <summary>
        /// Gets or sets the start date. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the start date from filter. Used for filtering new products. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The start date from.</value>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// Gets or sets the end date. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the response groups.
        /// </summary>
        /// <value>
        /// The response groups.
        /// </value>
        public virtual IList<string> ResponseGroups { get; set; }


        public virtual void Add(ISearchFilter filter)
        {
            if (filter != null)
            {
                Filters.Add(filter);
            }
        }

        public virtual void Apply(ISearchFilter filter)
        {
            if (filter != null)
            {
                CurrentFilters.Add(filter);
            }
        }
    }
}
