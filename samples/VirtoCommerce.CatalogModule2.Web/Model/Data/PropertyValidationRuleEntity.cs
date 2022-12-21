using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyValidationRuleEntity2 : PropertyValidationRuleEntity
    {
        public override PropertyValidationRule ToModel(PropertyValidationRule rule)
        {
            return base.ToModel(rule);
        }
        public override PropertyValidationRuleEntity FromModel(PropertyValidationRule rule)
        {
            return base.FromModel(rule);
        }
        public override void Patch(PropertyValidationRuleEntity target)
        {
            base.Patch(target);
        }
    }
}
