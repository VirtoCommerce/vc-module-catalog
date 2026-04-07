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
        private readonly IVideoProvider[] _providers;
        private readonly string _unsupportedUrlMessage;

        public VideoService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            IEnumerable<IVideoProvider> providers)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _providers = providers.ToArray();
            var providerNames = string.Join(", ", _providers.Select(x => x.Name));
            _unsupportedUrlMessage = $"Unsupported video URL. Available providers: {providerNames}.";
        }

        protected override async Task<IList<VideoEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return await ((ICatalogRepository)repository).Videos
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public virtual async Task<Video> CreateVideo(VideoCreateRequest createRequest)
        {
            await new VideoCreateRequestValidator().ValidateAndThrowAsync(createRequest);

            var provider = _providers.FirstOrDefault(x => x.CanHandle(createRequest.ContentUrl)) ??
                           throw new InvalidOperationException(_unsupportedUrlMessage);

            return await provider.GetVideoAsync(createRequest);
        }
    }
}
