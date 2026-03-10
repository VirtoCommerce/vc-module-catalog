using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class VideoService : CrudService<Video, VideoEntity, VideoChangingEvent, VideoChangedEvent>, IVideoService
    {
        private readonly IVideoProvider _videoProvider;

        public VideoService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            IVideoProvider videoProvider)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _videoProvider = videoProvider;
        }

        protected override async Task<IList<VideoEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return await ((ICatalogRepository)repository).Videos
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<Video> CreateVideo(VideoCreateRequest createRequest)
        {
            await new VideoCreateRequestValidator().ValidateAndThrowAsync(createRequest);

            return await _videoProvider.GetVideoAsync(createRequest);
        }
    }
}
