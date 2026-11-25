using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// The products' state in the store for the part, in the shape of the part's section, so that a state sent back changes
// nothing. It reads what the store's customers get, such as the prices of the store's price lists or the stock of the
// store's centers, never the data of every store.
public interface IProductSetupState : IProductSetupPart
{
    // The state of each product of the context, by product id; a product the part has nothing for may be left out.
    Task<IDictionary<string, object>> GetStatesAsync(ProductSetupContext context);
}
