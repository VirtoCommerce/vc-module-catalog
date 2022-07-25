using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IVideoService : ICrudService<Video>
    {
        Task<Video> CreateVideo(VideoCreateRequest createRequest);
    }
}
