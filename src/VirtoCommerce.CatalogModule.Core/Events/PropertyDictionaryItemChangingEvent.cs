using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class PropertyDictionaryItemChangingEvent : GenericChangedEntryEvent<PropertyDictionaryItem>
    {
        public PropertyDictionaryItemChangingEvent(IEnumerable<GenericChangedEntry<PropertyDictionaryItem>> changedEntries) : base(changedEntries)
        {
        }
    }
}
