using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Core.Events
{
    public class VideoChangeEvent : GenericChangedEntryEvent<Video>
    {
        public VideoChangeEvent(IEnumerable<GenericChangedEntry<Video>> changedEntries) : base(changedEntries)
        {
        }
    }
}
