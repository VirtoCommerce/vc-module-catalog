using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ExpTreeTests
    {

        [Fact]
        public void Test()
        {
            var expressionTree = AbstractTypeFactory<DynamicAssociationRuleTree>.TryCreateInstance();

            expressionTree.MergeFromPrototype(AbstractTypeFactory<DynamicAssociationRuleTreePrototype>.TryCreateInstance());

            var matchingRules = expressionTree.Children.OfType<BlockMatchingRules>();
            var resultingRules = expressionTree.Children.OfType<BlockResultingRules>();

            var dispRule = resultingRules.First();

            dispRule.Children.Add(new ConditionCategoryIs
            {
                CategoryIds = new[]
                {
                    "result category id",
                }
            });
            //dispRule.Children.Add(new ConditionPropertyValues
            //{
            //    PropertyNameValues = new Dictionary<string, string>
            //    {
            //        {"result property", "result value"}
            //    }
            //});

            var rule = matchingRules.First();

                rule.Children.Add(new ConditionCategoryIs
            {
                CategoryIds = new []
                {
                    "TestCategory",
                }
            });

            //rule.Children.Add(new ConditionPropertyValues
            //{
            //    PropertyNameValues = new Dictionary<string, string>
            //    {
            //        {"PropName", "Value1"}
            //    }
            //});

            var context = new DynamicAssociationExpressionEvaluationContext
            {
                Products = new List<CatalogProduct>
                {
                    new CatalogProduct
                    {
                        CategoryId = "TestCategory",
                        Properties = new List<Property>
                        {
                            new Property
                            {
                                Name = "PropName",
                                Values = new List<PropertyValue>
                                {
                                    new PropertyValue
                                    {
                                        Value = "Value1"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            foreach (var conditionRule in expressionTree.Children.OfType<BlockMatchingRules>())
            {
                var matchingRule = conditionRule;
                if (matchingRule.IsSatisfiedBy(context))
                {
                    var results = expressionTree.Children.OfType<BlockResultingRules>().First();

                    var cat = results.GetCategoryIds();
                    var propertyValues = results.GetPropertyValues();
                }

            }
        }
    }
}
