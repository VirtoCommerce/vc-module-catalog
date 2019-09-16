using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionRegistrar : IBulkUpdateActionRegistrar
    {
        private readonly ConcurrentDictionary<string, BulkUpdateActionDefinition> _knownActionTypes = new ConcurrentDictionary<string, BulkUpdateActionDefinition>();

        public IEnumerable<BulkUpdateActionDefinition> GetAll()
        {
            return _knownActionTypes.Values.ToArray();
        }

        public BulkUpdateActionDefinition GetByName(string name)
        {
            return _knownActionTypes.Values.FirstOrDefault(x => x.Name.EqualsInvariant(name));
        }

        public BulkUpdateActionDefinition Register(BulkUpdateActionDefinition definition)
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
