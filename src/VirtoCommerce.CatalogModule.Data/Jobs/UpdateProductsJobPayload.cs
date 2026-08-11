using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that resaves the products under categories whose hierarchy or visibility changed.
    /// </summary>
    public class UpdateProductsJobPayload
    {
        public List<string> CategoryIds { get; set; }
    }
}
