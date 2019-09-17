using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class ListEntryDataQuery
    {
        public ListEntry[] ListEntries { get; set; }
        public SearchCriteria SearchCriteria { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
