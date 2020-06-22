using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CoreModule.Core.Outlines;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DymanicAssociationExpressionTests
    {
        [Fact]
        public void AreItemsInCategory_MatchesCategoryId_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        CategoryId = "cat1",
                    }
                }
            };
            var categoryIds = new string[] { "cat1" };

            // Act
            var result = context.AreItemsInCategory(categoryIds);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemsInCategory_MatchesCategoryInOutline_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Outlines = new []
                        {
                            new Outline()
                            {
                                Items = new []
                                {
                                    new OutlineItem()
                                    {
                                        Id = "outerCat"
                                    },
                                    new OutlineItem()
                                    {
                                        Id = "innerCat"
                                    },
                                },
                            }
                        },
                    },
                }
            };
            var categoryIds = new string[] { "outerCat" };

            // Act
            var result = context.AreItemsInCategory(categoryIds);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemsInCategory_NotMatchingCategory_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        CategoryId = "innerCat",
                    },
                }
            };
            var categoryIds = new string[] { "noMatches" };

            // Act
            var result = context.AreItemsInCategory(categoryIds);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreItemsInCategory_NotMatchingOutline_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Outlines = new []
                        {
                            new Outline()
                            {
                                Items = new []
                                {
                                    new OutlineItem()
                                    {
                                        Id = "outerCat"
                                    },
                                    new OutlineItem()
                                    {
                                        Id = "innerCat"
                                    },
                                },
                            }
                        },
                    },
                }
            };
            var categoryIds = new string[] { "noMatches" };

            // Act
            var result = context.AreItemsInCategory(categoryIds);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreItemsInCategory_EmptyProduct_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct {},
                }
            };
            var categoryIds = new string[] { "noMatches" };

            // Act
            var result = context.AreItemsInCategory(categoryIds);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_SinglePropertyValueSingleConditionValue_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "IntProperty",
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = 1
                                    }
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "IntProperty", new[] { "1" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_MutliPropertyValueSingleConditionValue_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "IntProperty",
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = 1
                                    },
                                    new PropertyValue()
                                    {
                                        Value = 2
                                    },
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "IntProperty", new[] { "1" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_SinglePropertyValueMutliConditionValue_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "DictionaryProperty",
                                Dictionary = true,
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = "Elem1",
                                        Alias = "Elem1",
                                        ValueId = "Elem1Id",
                                    },
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "DictionaryProperty", new[] { "Elem1", "Elem2" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_MultiPropertyValueMutliConditionValue_Succeeded()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "DictionaryProperty",
                                Dictionary = true,
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = "Elem1",
                                        Alias = "Elem1",
                                        ValueId = "Elem1Id",
                                    },
                                    new PropertyValue()
                                    {
                                        Value = "Elem2",
                                        Alias = "Elem2",
                                        ValueId = "Elem2Id",
                                    },
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "DictionaryProperty", new[] { "Elem1", "Elem3" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_SinglePropertyValueMutliConditionValue_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "DictionaryProperty",
                                Dictionary = true,
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = "Elem1",
                                        Alias = "Elem1",
                                        ValueId = "Elem1Id",
                                    },
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "DictionaryProperty", new[] { "Elem2", "Elem3" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_NoPropertyWithName_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "IntProperty",
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = 1
                                    }
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "AnotherInt", new[] { "1" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreItemPropertyValuesEqual_DifferentPropertyValue_Failed()
        {
            // Arrange
            var context = new DynamicAssociationExpressionEvaluationContext()
            {
                Products = new[]
                {
                    new CatalogProduct
                    {
                        Properties = new [] {
                            new Property()
                            {
                                Name = "IntProperty",
                                Values = new []
                                {
                                    new PropertyValue()
                                    {
                                        Value = 1
                                    }
                                }
                            }
                        },
                    }
                }
            };
            var valuesToMatch = new Dictionary<string, string[]>
            {
                { "IntProperty", new[] { "2" } }
            };

            // Act
            var result = context.AreItemPropertyValuesEqual(valuesToMatch);

            // Assert
            Assert.False(result);
        }
    }
}
