using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations
{
    /// <summary>
    /// Represents the prototype for rule tree <see cref="DynamicAssociationRuleTree"/> containing the list of available rules for building a rule tree in UI
    /// </summary>
    public class DynamicAssociationRuleTreePrototype : ConditionTree
    {
        public DynamicAssociationRuleTreePrototype()
        {
            var matchingRules = new BlockMatchingRules()
                .WithAvailConditions(
                    new ConditionCategoryIs(),
                    new ConditionPropertyValues()
                );
            var resultingRules = new BlockResultingRules()
                .WithAvailConditions(
                    new ConditionCategoryIs(),
                    new ConditionPropertyValues()
                );
            WithAvailConditions(
                matchingRules,
                resultingRules
            );
            WithChildrens(
                matchingRules,
                resultingRules
            );
        }
    }
}
