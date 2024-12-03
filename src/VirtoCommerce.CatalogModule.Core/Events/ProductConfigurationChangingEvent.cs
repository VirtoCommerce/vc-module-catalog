using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events;

public class ProductConfigurationChangingEvent : GenericChangedEntryEvent<ProductConfiguration>
{
    public ProductConfigurationChangingEvent(IEnumerable<GenericChangedEntry<ProductConfiguration>> changedEntries)
        : base(changedEntries)
    {
    }
}
