using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionRegistrar : IBulkUpdateActionRegistrar
    {
        private readonly ConcurrentDictionary<string, IBulkUpdateActionDefinition> _knownActionTypes = new ConcurrentDictionary<string, IBulkUpdateActionDefinition>();

        public IEnumerable<IBulkUpdateActionDefinition> GetAll()
        {
            return _knownActionTypes.Values.ToArray();
        }

        public IBulkUpdateActionDefinition GetByName(string name)
        {
            return _knownActionTypes.Values.FirstOrDefault(x => x.Name.EqualsInvariant(name));
        }

        public IBulkUpdateActionDefinition Register(IBulkUpdateActionDefinition definition)
        {
            var actionName = definition.Name;

            if (!_knownActionTypes.ContainsKey(actionName))
            {
                _knownActionTypes.TryAdd(actionName, definition);
            }

            return _knownActionTypes[actionName];
        }
    }
}
