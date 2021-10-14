using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class VideoChangingEvent : GenericChangedEntryEvent<Video>
    {
        public VideoChangingEvent(IEnumerable<GenericChangedEntry<Video>> changedEntries) : base(changedEntries)
        {
        }
    }
}
