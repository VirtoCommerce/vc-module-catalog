using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// What keeps each product of the context from being bought in the store, as the storefront decides it. A finding's
// ObjectId is the product's id, and its category is the part's name, which the setup service sets whatever the part sends,
// so the part's read permission decides who sees it. A finding holds sentences only: never a secret, an exception message
// or a trace. Its fix names no tool: the tools add the tool and the back-office link. A critical finding means the product
// cannot be bought until it is fixed.
public interface IProductSetupCheck : IProductSetupPart
{
    Task<IList<McpFinding>> CheckAsync(ProductSetupContext context);
}
