using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Vimeo;

public partial class VimeoVideoProvider(IOptions<VideoOptions> videoOptions, IHttpClientFactory httpClientFactory)
    : IVideoProvider
{
    [GeneratedRegex("src=\"(.*?)\"")]
    private static partial Regex EmbedSrcRegex();

    private const int _maxDescriptionLength = 1024;
    private const string _oEmbedUrl = "https://vimeo.com/api/oembed.json";

    private readonly VideoOptions _videoOptions = videoOptions.Value;

    public bool CanHandle(string contentUrl)
    {
        if (string.IsNullOrWhiteSpace(contentUrl))
        {
            return false;
        }

        return Uri.TryCreate(contentUrl, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("vimeo.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(".vimeo.com", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Video> GetVideoAsync(VideoCreateRequest request)
    {
        var video = AbstractTypeFactory<Video>.TryCreateInstance();
        video.ContentUrl = request.ContentUrl;
        video.SortOrder = request.SortOrder.GetValueOrDefault();
        video.LanguageCode = request.LanguageCode;
        video.OwnerId = request.OwnerId;
        video.OwnerType = request.OwnerType;

        var requestUrl = $"{_oEmbedUrl}?url={Uri.EscapeDataString(request.ContentUrl)}";

        using var httpClient = httpClientFactory.CreateClient();

        if (!string.IsNullOrEmpty(_videoOptions.VimeoAccessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _videoOptions.VimeoAccessToken);
        }

        using var response = await httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrEmpty(_videoOptions.VimeoAccessToken))
            {
                throw new InvalidOperationException(
                    "Vimeo video is unavailable. Private videos require VimeoAccessToken in appsettings.json.");
            }

            throw new InvalidOperationException("Vimeo video not found.");
        }

        var content = await response.Content.ReadAsStringAsync();
        var resource = JObject.Parse(content);

        video.Name = resource.Value<string>("title");
        video.Description = resource.Value<string>("description")?.SoftTruncate(_maxDescriptionLength) ?? video.Name;
        video.ThumbnailUrl = resource.Value<string>("thumbnail_url");
        video.EmbedUrl = GetEmbedUrl(resource.Value<string>("html"));
        video.Duration = FormatDuration(resource.Value<int?>("duration"));
        video.UploadDate = resource.Value<DateTime?>("upload_date");

        return video;
    }

    private static string GetEmbedUrl(string html)
    {
        if (string.IsNullOrWhiteSpace(html) || !html.Contains("<iframe", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var match = EmbedSrcRegex().Match(html);

        return match.Success ? match.Groups[1].Value : null;
    }

    private static string FormatDuration(int? totalSeconds)
    {
        if (totalSeconds is null or 0)
        {
            return "00:00:00";
        }

        var time = TimeSpan.FromSeconds(totalSeconds.Value);
        return time.ToString(@"hh\:mm\:ss");
    }
}
