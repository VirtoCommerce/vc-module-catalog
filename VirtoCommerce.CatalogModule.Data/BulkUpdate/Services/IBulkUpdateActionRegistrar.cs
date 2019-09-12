using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public interface IBulkUpdateActionRegistrar
    {
        IBulkUpdateActionDefinition Register(IBulkUpdateActionDefinition definition);

        IBulkUpdateActionDefinition GetByName(string name);

        IEnumerable<IBulkUpdateActionDefinition> GetAll();
    }
}
