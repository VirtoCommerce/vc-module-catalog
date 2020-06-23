using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DynamicAssociationConditionSelectorTests
    {
        private readonly Mock<IDynamicAssociationSearchService> _dynamicAssociationSearchServiceMock;

        private readonly DynamicAssociationEvaluationContext _evaluationContext = new DynamicAssociationEvaluationContext();
        private readonly CatalogProduct _catalogProduct = new CatalogProduct();

        public DynamicAssociationConditionSelectorTests()
        {
            _dynamicAssociationSearchServiceMock = CreateDynamicAssociationSearchServiceMock();
        }

        [Fact]
        public async Task DynamicAssociationRule_Matching_Rule_Exception_Thrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new List<DynamicAssociation>
                    {
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new []
                                {
                                    new Mock<IConditionTree>().Object
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task DynamicAssociationRule_Result_Rule_Exception_Thrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new List<DynamicAssociation> {
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task DynamicAssociationRule_Output_Tuning_Block_Exception_Thrown()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new List<DynamicAssociation>
                    {
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules()
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct)
                );
        }

        [Fact]
        public async Task DynamicAssociationRule_SearchResultIsEmpty_Null_Returned()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult { Results = new List<DynamicAssociation>(), });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DynamicAssociationRule_Search_Succesed()
        {
            // Arrange
            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new List<DynamicAssociation>
                    {
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules(),
                                    new BlockOutputTuning(),
                                }
                            }
                        }
                    }
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.IsType<DynamicAssociationConditionEvaluationRequest>(result);
        }

        [Fact]
        public async Task DynamicAssociationRule_Find_First_And_Single_Rule()
        {
            // Arrange
            var matchingRuleMock = CreateBlockMatchingRulesMock();

            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new[]
                    {
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    CreateBlockMatchingRulesMock().Object,
                                    CreateBlockResultingRules(),
                                    new BlockOutputTuning(),
                                }
                            },
                        },
                        new DynamicAssociation
                        {
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    matchingRuleMock.Object,
                                }
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.IsType<DynamicAssociationConditionEvaluationRequest>(result);
            matchingRuleMock.Verify(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()), Times.Never);
        }

        [Fact]
        public async Task DynamicAssociationRule_Select_Only_Not_Expired_Not_Started_Yet()
        {
            // Arrange
            var matchingRule = CreateBlockMatchingRulesMock();

            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult {
                    Results = new[]
                    {
                        new DynamicAssociation
                        {
                            StartDate = DateTime.Now.AddMinutes(1),
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    matchingRule.Object,
                                }
                            }
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.Null(result);
            matchingRule.Verify(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()), Times.Never());
        }

        [Fact]
        public async Task DynamicAssociationRule_Select_Only_Not_Expired_Already_Ended()
        {
            // Arrange
            var matchingRule = CreateBlockMatchingRulesMock();

            _dynamicAssociationSearchServiceMock
                .Setup(x => x.SearchDynamicAssociationsAsync(It.IsAny<DynamicAssociationSearchCriteria>()))
                .ReturnsAsync(new DynamicAssociationSearchResult
                {
                    Results = new[]
                    {
                        new DynamicAssociation
                        {
                            EndDate = DateTime.Now.AddMinutes(-1),
                            ExpressionTree = new DynamicAssociationRuleTree
                            {
                                Children = new IConditionTree[]
                                {
                                    matchingRule.Object,
                                },
                            },
                        },
                    },
                });

            var selector = CreateDynamicAssociationConditionSelector();

            // Act
            var result = await selector.GetDynamicAssociationConditionAsync(_evaluationContext, _catalogProduct);

            // Assert
            Assert.Null(result);
            matchingRule.Verify(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()), Times.Never);
        }


        private static Mock<IDynamicAssociationSearchService> CreateDynamicAssociationSearchServiceMock()
        {
            var result = new Mock<IDynamicAssociationSearchService>();

            return result;
        }

        private IDynamicAssociationConditionSelector CreateDynamicAssociationConditionSelector()
        {
            var result = new DynamicAssociationConditionsSelector(_dynamicAssociationSearchServiceMock.Object);

            return result;
        }

        private static Mock<BlockMatchingRules> CreateBlockMatchingRulesMock()
        {
            var result = new Mock<BlockMatchingRules>();
            result
                .Setup(x => x.IsSatisfiedBy(It.IsAny<IEvaluationContext>()))
                .Returns(true);

            return result;
        }

        private static BlockResultingRules CreateBlockResultingRules()
        {
            var result = new BlockResultingRules
            {
                Children = new IConditionTree[]
                {
                    new ConditionPropertyValues
                    {
                        Properties = new[]
                        {
                            new Property
                            {
                                Name = string.Empty,
                                Values = new []
                                {
                                    new PropertyValue { Value = string.Empty, }
                                }
                            }
                        }
                    },
                    new ConditionCategoryIs { CategoryIds = Array.Empty<string>(), }
                }
            };

            return result;
        }
    }
}
