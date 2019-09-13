using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public interface IBulkUpdateActionRegistrar
    {
        BulkUpdateActionDefinition Register(BulkUpdateActionDefinition definition);

        BulkUpdateActionDefinition GetByName(string name);

        IEnumerable<BulkUpdateActionDefinition> GetAll();
    }
}
