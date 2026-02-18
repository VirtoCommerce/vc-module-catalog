using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    /// <summary>
    /// Provides video metadata from external video hosting platforms.
    /// </summary>
    public interface IVideoProvider
    {
        Task<Video> GetVideoAsync(VideoCreateRequest request);
    }
}
