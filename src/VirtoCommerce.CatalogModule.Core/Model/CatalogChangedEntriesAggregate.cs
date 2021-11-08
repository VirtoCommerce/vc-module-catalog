using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class CatalogChangedEntriesAggregate
    {
        public IEnumerable<GenericChangedEntry<Catalog>> CatalogEntries { get; set; }

        public IEnumerable<GenericChangedEntry<Category>> CategoryEntries { get; set; }

        public IEnumerable<GenericChangedEntry<CatalogProduct>> ProductEntries { get; set; }
    }
}
