using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VirtoCommerce.CatalogModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

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

    [Fact]
    public void TryInheritFrom_CategoryParent_ProductAndVariationProperties_ShouldNotDuplicate()
    {
        // Arrange
        var product = new CatalogProduct();

        // Category has multiple properties with the same name but with different types
        var parent = new Category();
        AddProperty(parent, "Color", PropertyType.Variation);
        AddProperty(parent, "Color", PropertyType.Product);
        AddProperty(parent, "Color", PropertyType.Category);
        AddProperty(parent, "Color", PropertyType.Catalog);

        // Act
        product.TryInheritFrom(parent);

        // Assert
        product.Properties.Should().HaveCount(3);
        var propertyTypes = product.Properties.Select(x => x.Type).ToArray();

        // Variation and Product properties should not duplicate, the last of them should be inherited
        propertyTypes.Should().BeEquivalentTo([PropertyType.Catalog, PropertyType.Category, PropertyType.Product]);
    }


    private static T CreateObjectWithPropertyValues<T>(IList<string> values = null, PropertyType propertyType = PropertyType.Product)
        where T : IHasProperties, new()
    {
        var result = new T();
        var property = AddProperty(result, "Color", propertyType);

        property.Values = values
            ?.Select(x => new PropertyValue
            {
                PropertyName = property.Name,
                Value = x,
            })
            .ToList();

        return result;
    }

    private static Property AddProperty<T>(T owner, string propertyName, PropertyType propertyType)
        where T : IHasProperties
    {
        var property = new Property
        {
            Name = propertyName,
            Type = propertyType,
        };

        owner.Properties ??= [];
        owner.Properties.Add(property);

        return property;
    }
}
