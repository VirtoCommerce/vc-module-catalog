using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events;

public class ProductConfigurationChangedEvent : GenericChangedEntryEvent<ProductConfiguration>
{
    public ProductConfigurationChangedEvent(IEnumerable<GenericChangedEntry<ProductConfiguration>> changedEntries)
        : base(changedEntries)
    {
    }
}
