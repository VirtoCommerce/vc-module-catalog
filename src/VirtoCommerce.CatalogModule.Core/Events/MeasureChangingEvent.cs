using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class MeasureChangingEvent : GenericChangedEntryEvent<Measure>
    {
        public MeasureChangingEvent(IEnumerable<GenericChangedEntry<Measure>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
