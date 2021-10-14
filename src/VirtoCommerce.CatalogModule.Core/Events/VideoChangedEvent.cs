using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class VideoChangedEvent : GenericChangedEntryEvent<Video>
    {
        public VideoChangedEvent(IEnumerable<GenericChangedEntry<Video>> changedEntries) : base(changedEntries)
        {
        }
    }
}
