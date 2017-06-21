using System;
using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchCriteria : CatalogSearchCriteria
    {
        public string[] Terms { get; set; }

        public string Currency { get; set; }

        public string[] Pricelists { get; set; }

        public NumericRange PriceRange { get; set; }

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
    }
}
