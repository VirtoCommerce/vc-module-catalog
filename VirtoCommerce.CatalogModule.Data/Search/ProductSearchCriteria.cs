using System;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchCriteria : CatalogSearchCriteria
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Product;

        public virtual string Currency { get; set; }
        public virtual IList<string> Pricelists { get; set; }
        public NumericRange PriceRange { get; set; }

        public virtual IList<IBrowseFilter> BrowseFilters { get; set; }
        public virtual IList<IBrowseFilter> CurrentFilters { get; set; }

        /// <summary>
        /// Gets or sets the class types.
        /// </summary>
        /// <value>The class types.</value>
        public virtual IList<string> ClassTypes { get; set; } = new List<string>();

        /// <summary>
        /// Specifies if we search for hidden products.
        /// </summary>
        public virtual bool WithHidden { get; set; }

        /// <summary>
        /// Gets or sets the start date. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The start date.</value>
        public virtual DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the start date from filter. Used for filtering new products. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The start date from.</value>
        public virtual DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// Gets or sets the end date. The date must be in UTC format as that is format indexes are stored in.
        /// </summary>
        /// <value>The end date.</value>
        public virtual DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the response groups.
        /// </summary>
        /// <value>
        /// The response groups.
        /// </value>
        public virtual IList<string> ResponseGroups { get; set; }
        public virtual ItemResponseGroup ResponseGroup { get; set; }
    }
}
