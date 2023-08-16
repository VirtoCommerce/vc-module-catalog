using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class PropertyDictionaryItemChangedEvent : GenericChangedEntryEvent<PropertyDictionaryItem>
    {
        public PropertyDictionaryItemChangedEvent(IEnumerable<GenericChangedEntry<PropertyDictionaryItem>> changedEntries) : base(changedEntries)
        {
        }
    }
}
