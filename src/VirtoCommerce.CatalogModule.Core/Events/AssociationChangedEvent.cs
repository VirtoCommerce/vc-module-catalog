using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class AssociationChangedEvent : GenericChangedEntryEvent<ProductAssociation>
    {
        public AssociationChangedEvent(IEnumerable<GenericChangedEntry<ProductAssociation>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
