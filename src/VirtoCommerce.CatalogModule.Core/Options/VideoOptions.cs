namespace VirtoCommerce.CatalogModule.Core.Options
{
    public class VideoOptions
    {
        public const string SectionName = "Videos";

        /// <summary>
        /// An API key is a unique string that lets you access an Google YouTube Data API
        /// </summary>
        public string GoogleApiKey { get; set; }

        /// <summary>
        /// An access token for Vimeo API. Required for private videos.
        /// </summary>
        public string VimeoAccessToken { get; set; }
    }
}
