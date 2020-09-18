using FluentAssertions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class ProductAssociationValidatorTest
    {
        [Fact]
        public void ValidateAssociation_ValidItem_Success()
        {
            var validator = new ProductAssociationValidator();
            var association = new ProductAssociation()
            {
                ItemId = "ItemId",
                AssociatedObjectId = "AssociatedObjectId",
                AssociatedObjectType = "AssociatedObjectType"
            };

            //Act

            var result = validator.Validate(association);

            //Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateAssociation_NotValidItem_Failed()
        {
            var validator = new ProductAssociationValidator();
            var association = new ProductAssociation();

            //Act

            var result = validator.Validate(association);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(6);

            result.Errors.Should().Contain(x => x.PropertyName == "ItemId" && x.ErrorCode == "NotNullValidator");
            result.Errors.Should().Contain(x => x.PropertyName == "ItemId" && x.ErrorCode == "NotEmptyValidator");

            result.Errors.Should().Contain(x => x.PropertyName == "AssociatedObjectId" && x.ErrorCode == "NotNullValidator");
            result.Errors.Should().Contain(x => x.PropertyName == "AssociatedObjectId" && x.ErrorCode == "NotEmptyValidator");

            result.Errors.Should().Contain(x => x.PropertyName == "AssociatedObjectType" && x.ErrorCode == "NotNullValidator");
            result.Errors.Should().Contain(x => x.PropertyName == "AssociatedObjectType" && x.ErrorCode == "NotEmptyValidator");
        }
    }
}
