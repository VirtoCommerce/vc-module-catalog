using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Web.Model
{
    public class MoveInfo
    {
        public string Catalog { get; set; }

        public string Category { get; set; }

        public ICollection<ListEntry> ListEntries { get; set; }
    }
}
