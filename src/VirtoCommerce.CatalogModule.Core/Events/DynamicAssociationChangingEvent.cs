using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class DynamicAssociationChangingEvent : GenericChangedEntryEvent<DynamicAssociation>
    {
        public DynamicAssociationChangingEvent(IEnumerable<GenericChangedEntry<DynamicAssociation>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
