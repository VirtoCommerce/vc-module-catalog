using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    /// <summary>
    /// Provides video metadata from external video hosting platforms.
    /// </summary>
    public interface IVideoProvider
    {
        string Name { get; }
        bool CanHandle(string contentUrl);
        Task<Video> GetVideoAsync(VideoCreateRequest request);
    }
}
