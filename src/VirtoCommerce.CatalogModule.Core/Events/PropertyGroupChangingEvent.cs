using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class PropertyGroupChangingEvent : GenericChangedEntryEvent<PropertyGroup>
    {
        public PropertyGroupChangingEvent(IEnumerable<GenericChangedEntry<PropertyGroup>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
