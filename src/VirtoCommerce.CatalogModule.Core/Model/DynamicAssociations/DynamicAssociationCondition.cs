using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations
{
    public class DynamicAssociationCondition
    {
        public ICollection<string> CategoryIds { get; set; }
        public IDictionary<string, string> PropertyValues { get; set; }
    }
}
