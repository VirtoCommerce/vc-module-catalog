using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Options;
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
        private readonly VideoOptions _videoOptions;

        public VideoService(IOptions<VideoOptions> videoOptions,
            Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _videoOptions = videoOptions.Value;
        }

        protected override async Task<IEnumerable<VideoEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((ICatalogRepository)repository).Videos
                .Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<Video> CreateVideo(VideoCreateRequest createRequest)
        {
            await new VideoCreateRequestValidator().ValidateAndThrowAsync(createRequest);

            var video = AbstractTypeFactory<Video>.TryCreateInstance();
            video.ContentUrl = createRequest.ContentUrl;
            video.SortOrder = createRequest.SortOrder.GetValueOrDefault();
            video.LanguageCode = createRequest.LanguageCode;
            video.OwnerId = createRequest.OwnerId;
            video.OwnerType = createRequest.OwnerType;

            using var service =
                new YouTubeService(new BaseClientService.Initializer { ApiKey = _videoOptions.GoogleApiKey });
            if (!string.IsNullOrEmpty(service.ApiKey))
            {
                var request = service.Videos.List(new[] { "snippet", "contentDetails", "player" });
                request.Id = GetVideoId(createRequest.ContentUrl);
                var response = await request.ExecuteAsync();
                if (response.Items == null || response.Items.Count == 0)
                    throw new InvalidOperationException("Youtube video not found.");

                var resource = response.Items[0];
                var snippet = resource.Snippet;

                video.Name = snippet.Title;
                video.Description = snippet.Description;
                video.UploadDate = snippet.PublishedAt;
                video.ThumbnailUrl = snippet.Thumbnails.High.Url;
                video.EmbedUrl = GetEmbedUrl(resource.Player.EmbedHtml);
                video.Duration = FormatDuration(resource.ContentDetails.Duration);
            }
            else
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://www.youtube.com/oembed?url={createRequest.ContentUrl}&format=json");
                using var response = await service.HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var resource = JObject.Parse(content);
                video.Name = video.Description = resource.Value<string>("title");
                video.ThumbnailUrl = resource.Value<string>("thumbnail_url");
                video.EmbedUrl = GetEmbedUrl(resource.Value<string>("html"));
                video.Duration = "00:00:00";
            }

            return video;
        }

        private static string GetVideoId(string contentUrl)
        {
            if (string.IsNullOrWhiteSpace(contentUrl))
                return null;

            var parts = Regex.Split(contentUrl, @"(vi\/|v%3D|v=|\/v\/|youtu\.be\/|\/embed\/)");
            return parts.Length > 2 && !string.IsNullOrEmpty(parts[2])
                ? Regex.Split(parts[2], @"[^0-9a-z_\-]", RegexOptions.IgnoreCase)[0]
                : parts[0];
        }

        private static string GetEmbedUrl(string html)
        {
            if (string.IsNullOrWhiteSpace(html) || !html.Contains("<iframe", StringComparison.OrdinalIgnoreCase))
                return null;

            var match = Regex.Match(html, "src=\"(.*?)\"", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string FormatDuration(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
                return null;

            var match = Regex.Match(duration, @"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?$");
            if (!match.Success)
                return duration;

            return string.Join(":", match.Groups.Values.Skip(1)
                .Select(grp => grp.Value)
                .Select(val => string.IsNullOrEmpty(val) ? "00" : val.PadLeft(2, '0')));
        }
    }
}
