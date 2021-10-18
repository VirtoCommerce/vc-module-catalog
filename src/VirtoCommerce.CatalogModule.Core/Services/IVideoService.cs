using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IVideoService
    {
        Task<Video> CreateVideo(VideoCreateRequest createRequest);
    }
}
