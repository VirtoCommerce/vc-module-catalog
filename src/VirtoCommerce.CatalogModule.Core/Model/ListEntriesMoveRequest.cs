using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    /// <summary>
    /// Represents  move list entries command
    /// </summary>
    public class ListEntriesMoveRequest : ValueObject, IHasCatalogId
    {
        //Destination catalog
        public string Catalog { get; set; }
        public string CatalogId => Catalog;
        //Destination category
        public string Category { get; set; }

        public ICollection<ListEntryBase> ListEntries { get; set; }
    }
}
