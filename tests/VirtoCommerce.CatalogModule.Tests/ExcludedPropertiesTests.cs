using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class ExcludedPropertiesTests
    {
        [Fact]
        public void ExcludedPropertyMustBeExludedForAllNestedObjects()
        {
            //Arrange
            var color = new Property { Id = "color", Name = "color" };
            var size = new Property { Id = "size", Name = "size" };
            var catalog = new Catalog
            {
                Properties = new Property[] { color, size }
            };
            var category = new Category { };
            var variation = new Variation() { };
            var product = new CatalogProduct { Variations = new[] { variation } };

            //Act
            category.ExcludedProperties = new[] { new ExcludedProperty { Name = size.Name } };

            category.TryInheritFrom(catalog);
            product.TryInheritFrom(category);
            variation.TryInheritFrom(product);

            //Assertion

            Assert.Single(category.Properties);
            Assert.Contains(color, category.Properties);

            Assert.Single(product.Properties);
            Assert.Contains(color, product.Properties);
            var excludedProp = product.ExcludedProperties.FirstOrDefault();
            Assert.NotNull(excludedProp);
            Assert.True(excludedProp.Name == size.Name);
            Assert.True(excludedProp.IsInherited);

            Assert.Single(variation.Properties);
            Assert.Contains(color, variation.Properties);

        }

    }
}
