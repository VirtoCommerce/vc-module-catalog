using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class PropertyValidatorsTests
    {
        [Theory]
        [MemberData(nameof(GetPropertyNameData))]
        public void ValidatePropertyNames(string propertyName, bool isNew, bool isValid)
        {
            // Arrange
            var property = new Property()
            {
                IsNew = isNew,
                Name = propertyName,
            };
            // Act
            var validator = new PropertyValidator();

            // Assert
            var result = validator.Validate(property);
            Assert.Equal(isValid, result.IsValid);
        }

        [Theory]
        [MemberData(nameof(GetPropertyNameData))]
        public void ValidateNonManageablePropertyNamesForProduct(string propertyName, bool isNew, bool isValid)
        {
            // Arrange
            var property = new Property()
            {
                Name = propertyName,
                IsNew = isNew,
            };

            var product = new CatalogProduct()
            {
                Id = "id",
                Name = "name",
                Code = "code",
                CatalogId = "catalogId",
                Properties = new List<Property> { property }
            };

            // Act
            var validator = new ProductValidator();

            // Assert
            var result = validator.Validate(product);
            Assert.Equal(isValid, result.IsValid);
        }

        public static IEnumerable<object[]> GetPropertyNameData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    // property name, is new, expected result 
                    "New", true, true
                },
                new object[]
                {
                    "New1", true, true
                },
                new object[]
                {
                    "New1_property2", true, true
                },
                new object[]
                {
                    "_new1", true, false,
                },
                new object[]
                {
                    "new1_", true, false
                },
                new object[]
                {
                    "тест", true, false
                },
                new object[]
                {
                    "existing property name", false, false
                },
            };
        }
    }
}
