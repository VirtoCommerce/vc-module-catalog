using System;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Services;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

// The page size the MCP module allows: 20 by default, at most 50.
public class FixedPageSizeResolver : IMcpPageSizeResolver
{
    public Task<int> GetPageSizeAsync(int? take)
    {
        return Task.FromResult(Math.Clamp(take ?? 20, 1, 50));
    }
}
