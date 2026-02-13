using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VirtoCommerce.CatalogModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductPropertyValueInheritanceTests
    {
        [Fact]
        public void TryInheritFrom_ProductParent_ParentHasNoValues_ShouldNotInheritValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<CatalogProduct>();

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().BeNullOrEmpty();
        }

        [Fact]
        public void TryInheritFrom_ProductParent_ShouldInheritPropertyValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<CatalogProduct>(["Red"]);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().HaveCount(1);
            property.Values.First().Value.Should().Be("Red");
            property.Values.First().IsInherited.Should().BeTrue();
        }

        [Fact]
        public void TryInheritFrom_ProductParent_MultipleValues_ShouldInheritAllValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<CatalogProduct>(["Red", "Green"]);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().HaveCount(2);
            property.Values.Select(x => x.Value).Should().BeEquivalentTo(["Red", "Green"]);
        }

        [Fact]
        public void TryInheritFrom_ProductParent_ChildHasOwnValues_ShouldNotOverwrite()
        {
            // Arrange
            var product = CreateObjectWithPropertyValues<CatalogProduct>(["Green"]);
            var parent = CreateObjectWithPropertyValues<CatalogProduct>(["Red"]);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().HaveCount(1);
            property.Values.First().Value.Should().Be("Green");
        }

        [Fact]
        public void TryInheritFrom_ProductParent_VariationProperty_ShouldNotInheritValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<CatalogProduct>(["Red"], PropertyType.Variation);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().BeNullOrEmpty();
        }

        [Fact]
        public void TryInheritFrom_CategoryParent_ShouldNotInheritPropertyValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<Category>(["Red"]);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().BeNullOrEmpty();
        }

        [Fact]
        public void TryInheritFrom_CatalogParent_ShouldNotInheritPropertyValues()
        {
            // Arrange
            var product = new CatalogProduct();
            var parent = CreateObjectWithPropertyValues<Catalog>(["Red"]);

            // Act
            product.TryInheritFrom(parent);

            // Assert
            product.Properties.Should().HaveCount(1);
            var property = product.Properties.First();
            property.Values.Should().BeNullOrEmpty();
        }


        private static T CreateObjectWithPropertyValues<T>(IList<string> values = null, PropertyType propertyType = PropertyType.Product)
            where T : IHasProperties, new()
        {
            const string propertyName = "Color";

            var parent = new T
            {
                Properties =
                [
                    new Property
                    {
                        Name = propertyName,
                        Type = propertyType,
                        ValueType = PropertyValueType.ShortText,
                        Values = values
                            ?.Select(x => new PropertyValue
                            {
                                PropertyName = propertyName,
                                Value = x,
                                ValueType = PropertyValueType.ShortText,
                            })
                            .ToList(),
                    },
                ],
            };

            return parent;
        }
    }
}
