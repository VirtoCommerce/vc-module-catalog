using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.YouTube
{
    public class YouTubeVideoProvider : IVideoProvider
    {
        private readonly VideoOptions _videoOptions;

        public YouTubeVideoProvider(IOptions<VideoOptions> videoOptions)
        {
            _videoOptions = videoOptions.Value;
        }

        public async Task<Video> GetVideoAsync(VideoCreateRequest request)
        {
            var video = AbstractTypeFactory<Video>.TryCreateInstance();
            video.ContentUrl = request.ContentUrl;
            video.SortOrder = request.SortOrder.GetValueOrDefault();
            video.LanguageCode = request.LanguageCode;
            video.OwnerId = request.OwnerId;
            video.OwnerType = request.OwnerType;

            using var service = new YouTubeService(new BaseClientService.Initializer { ApiKey = _videoOptions.GoogleApiKey });

            if (!string.IsNullOrEmpty(service.ApiKey))
            {
                var apiRequest = service.Videos.List(new[] { "snippet", "contentDetails", "player" });
                apiRequest.Id = GetVideoId(request.ContentUrl);
                var response = await apiRequest.ExecuteAsync();

                if (response.Items == null || response.Items.Count == 0)
                {
                    throw new InvalidOperationException("Youtube video not found.");
                }

                var resource = response.Items[0];
                var snippet = resource.Snippet;

                video.Name = snippet.Title;
                video.Description = snippet.Description;
                video.UploadDate = snippet.PublishedAtDateTimeOffset?.UtcDateTime;
                video.ThumbnailUrl = snippet.Thumbnails.High.Url;
                video.EmbedUrl = GetEmbedUrl(resource.Player.EmbedHtml);
                video.Duration = FormatDuration(resource.ContentDetails.Duration);
            }
            else
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://www.youtube.com/oembed?url={request.ContentUrl}&format=json");
                using var response = await service.HttpClient.SendAsync(httpRequest);
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
            {
                return null;
            }

            var parts = Regex.Split(contentUrl, @"(vi\/|v%3D|v=|\/v\/|youtu\.be\/|\/embed\/)");

            return parts.Length > 2 && !string.IsNullOrEmpty(parts[2])
                ? Regex.Split(parts[2], @"[^0-9a-z_\-]", RegexOptions.IgnoreCase)[0]
                : parts[0];
        }

        private static string GetEmbedUrl(string html)
        {
            if (string.IsNullOrWhiteSpace(html) || !html.Contains("<iframe", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var match = Regex.Match(html, "src=\"(.*?)\"", RegexOptions.Multiline);

            return match.Success ? match.Groups[1].Value : null;
        }

        private static string FormatDuration(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
            {
                return null;
            }

            var match = Regex.Match(duration, @"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?$");

            if (!match.Success)
            {
                return duration;
            }

            return string.Join(":", match.Groups.Values.Skip(1)
                .Select(grp => grp.Value)
                .Select(val => string.IsNullOrEmpty(val) ? "00" : val.PadLeft(2, '0')));
        }
    }
}
