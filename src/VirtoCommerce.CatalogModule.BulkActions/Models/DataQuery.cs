using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.BulkActions.Models
{

    public class DataQuery
    {
        /// <summary>
        /// Gets or sets the list entries.
        /// </summary>
        public ListEntryBase[] ListEntries { get; set; }

        /// <summary>
        /// Gets or sets the search criteria.
        /// </summary>
        public CatalogListEntrySearchCriteria SearchCriteria { get; set; }

        /// <summary>
        /// Gets or sets the skip.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the take.
        /// </summary>
        public int? Take { get; set; }
    }
}
