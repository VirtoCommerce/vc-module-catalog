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
        public void AreItemPropertyValuesEqual_NotMatchingCategory_Failed()
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
        public void AreItemPropertyValuesEqual_NotMatchingOutline_Failed()
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
        public void AreItemPropertyValuesEqual_EmptyProduct_Failed()
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
    }
}
