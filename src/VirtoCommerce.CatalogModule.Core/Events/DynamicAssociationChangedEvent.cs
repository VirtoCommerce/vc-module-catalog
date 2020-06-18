using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class DynamicAssociationChangedEvent : GenericChangedEntryEvent<DynamicAssociation>
    {
        public DynamicAssociationChangedEvent(IEnumerable<GenericChangedEntry<DynamicAssociation>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
