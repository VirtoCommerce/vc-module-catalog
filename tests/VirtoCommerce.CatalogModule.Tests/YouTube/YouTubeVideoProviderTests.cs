using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Data.YouTube;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.YouTube;

/// <summary>
/// Integration tests for <see cref="YouTubeVideoProvider"/>.
/// These tests make real HTTP requests to the YouTube oEmbed API and require internet access.
/// Tests that use the YouTube Data API require a Google API key stored in user secrets or
/// the <c>Videos__GoogleApiKey</c> environment variable.
/// To configure: <c>dotnet user-secrets set "Videos:GoogleApiKey" "YOUR_KEY"</c>
/// </summary>
[Trait("Category", "IntegrationTest")]
public class YouTubeVideoProviderTests
{
    // https://www.youtube.com/watch?v=2liEotbjT1I
    private const string KnownVideoUrl = "https://www.youtube.com/watch?v=2liEotbjT1I";

    /// <summary>
    /// API key read from user secrets (<c>Videos:GoogleApiKey</c>) or the
    /// <c>Videos__GoogleApiKey</c> environment variable. Null when not configured.
    /// </summary>
    private static readonly string _apiKey = new ConfigurationBuilder()
        .AddUserSecrets<YouTubeVideoProviderTests>()
        .AddEnvironmentVariables()
        .Build()[$"{VideoOptions.SectionName}:GoogleApiKey"];

    private static YouTubeVideoProvider CreateProvider(string apiKey = null)
    {
        var options = Options.Create(new VideoOptions { GoogleApiKey = apiKey });
        return new YouTubeVideoProvider(options);
    }

    private static VideoCreateRequest CreateRequest(
        string contentUrl,
        int? sortOrder = null,
        string languageCode = null,
        string ownerId = null,
        string ownerType = null)
    {
        return new VideoCreateRequest
        {
            ContentUrl = contentUrl,
            SortOrder = sortOrder,
            LanguageCode = languageCode,
            OwnerId = ownerId,
            OwnerType = ownerType,
        };
    }

    [Fact]
    public async Task GetVideoAsync_ValidUrl_ReturnsPopulatedVideoMetadata()
    {
        // Arrange
        var provider = CreateProvider();
        var request = CreateRequest(KnownVideoUrl);

        // Act
        var video = await provider.GetVideoAsync(request);

        // Assert
        video.Should().NotBeNull();
        video.Name.Should().NotBeNullOrEmpty();
        video.Description.Should().NotBeNullOrEmpty();
        video.ThumbnailUrl.Should().NotBeNullOrEmpty();
        video.EmbedUrl.Should().NotBeNullOrEmpty().And.StartWith("https://");
    }

    [Fact]
    public async Task GetVideoAsync_ValidUrl_MapsRequestFieldsToVideo()
    {
        // Arrange
        var provider = CreateProvider();
        var request = CreateRequest(
            KnownVideoUrl,
            sortOrder: 5,
            languageCode: "en-US",
            ownerId: "owner-1",
            ownerType: "CatalogProduct");

        // Act
        var video = await provider.GetVideoAsync(request);

        // Assert
        video.ContentUrl.Should().Be(KnownVideoUrl);
        video.SortOrder.Should().Be(5);
        video.LanguageCode.Should().Be("en-US");
        video.OwnerId.Should().Be("owner-1");
        video.OwnerType.Should().Be("CatalogProduct");
    }

    [Fact]
    public async Task GetVideoAsync_OEmbedPath_DurationIsZero()
    {
        // Arrange — no API key forces the oEmbed fallback, which does not return duration
        var provider = CreateProvider();
        var request = CreateRequest(KnownVideoUrl);

        // Act
        var video = await provider.GetVideoAsync(request);

        // Assert
        Assert.Equal("00:00:00", video.Duration);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=2liEotbjT1I")]
    [InlineData("https://youtu.be/2liEotbjT1I")]
    public async Task GetVideoAsync_VariousUrlFormats_ReturnsVideo(string contentUrl)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            Assert.Skip("Google API key not configured. Set Videos:GoogleApiKey in user secrets or the Videos__GoogleApiKey environment variable.");
        }

        // Arrange
        var provider = CreateProvider(_apiKey);
        var request = CreateRequest(contentUrl);

        // Act
        var video = await provider.GetVideoAsync(request);

        // Assert
        video.Should().NotBeNull();
        video.Name.Should().NotBeNullOrEmpty();
        video.ContentUrl.Should().Be(contentUrl);
    }

    [Fact]
    public Task GetVideoAsync_NonExistentVideoId_ThrowsHttpRequestException()
    {
        // Arrange
        var provider = CreateProvider();
        var request = CreateRequest("https://www.youtube.com/watch?v=XXXXXXXXXXX_INVALID");

        // Act
        var act = () => provider.GetVideoAsync(request);

        // Assert — YouTube oEmbed returns 404 for non-existent videos
        return act.Should().ThrowAsync<HttpRequestException>();
    }
}
