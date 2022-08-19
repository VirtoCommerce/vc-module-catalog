using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class VideoOwnerChangingEventHandler : IEventHandler<ProductChangingEvent>
    {
        private readonly IVideoSearchService _videoSearchService;
        private readonly IVideoService _videoService;

        public VideoOwnerChangingEventHandler(IVideoSearchService videoSearchService, IVideoService videoService)
        {
            _videoSearchService = videoSearchService;
            _videoService = videoService;
        }

        public async Task Handle(ProductChangingEvent message)
        {
            var ownerIds = message.ChangedEntries
                .Where(x => x.EntryState == EntryState.Deleted)
                .Select(x => x.OldEntry.Id)
                .ToList();

            if (!ownerIds.Any())
            {
                return;
            }

            var searchCriteria = new VideoSearchCriteria
            {
                OwnerIds = ownerIds,
                OwnerType = KnownDocumentTypes.Product,
            };

            var searchResult = await _videoSearchService.SearchAsync(searchCriteria);

            if (searchResult.TotalCount != 0)
            {
                await _videoService.DeleteAsync(searchResult.Results.Select(x => x.Id));
            }
        }
    }
}
