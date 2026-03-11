using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class CompositeVideoProvider(IEnumerable<IVideoProvider> providers) : IVideoProvider
{
    public bool CanHandle(string contentUrl)
    {
        return providers.Any(p => p.CanHandle(contentUrl));
    }

    public Task<Video> GetVideoAsync(VideoCreateRequest request)
    {
        var provider = providers.FirstOrDefault(p => p.CanHandle(request.ContentUrl))
            ?? throw new InvalidOperationException("Unsupported video URL. Supported: YouTube, Vimeo.");

        return provider.GetVideoAsync(request);
    }
}
