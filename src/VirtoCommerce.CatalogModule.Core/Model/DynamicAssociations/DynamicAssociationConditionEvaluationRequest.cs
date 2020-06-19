using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations
{
    public class DynamicAssociationConditionEvaluationRequest
    {
        public string Keyword { get; set; }

        public ICollection<string> CategoryIds { get; set; }

        public IDictionary<string, string[]> PropertyValues { get; set; }

        public string Sort { get; set; }

        public int Take { get; set; } = 20;

        public int Skip { get; set; } = 0;

    }
}
