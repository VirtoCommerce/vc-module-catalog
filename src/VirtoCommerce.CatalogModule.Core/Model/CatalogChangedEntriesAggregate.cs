using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public sealed class CatalogChangedEntriesAggregate
    {
        public List<GenericChangedEntry<Catalog>> CatalogEntries { get; set; }

        public List<GenericChangedEntry<Category>> CategoryEntries { get; set; }

        public List<GenericChangedEntry<CatalogProduct>> ProductEntries { get; set; }
    }
}
