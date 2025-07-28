using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events;

public class AutomaticLinkQueryChangingEvent(IEnumerable<GenericChangedEntry<AutomaticLinkQuery>> changedEntries)
    : GenericChangedEntryEvent<AutomaticLinkQuery>(changedEntries);
