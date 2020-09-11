using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class ProductAssociationValidator : AbstractValidator<ProductAssociation>
    {
        public ProductAssociationValidator()
        {
            RuleFor(association => association.ItemId).NotNull().NotEmpty();
            RuleFor(association => association.AssociatedObjectId).NotNull().NotEmpty();
            RuleFor(association => association.AssociatedObjectType).NotNull().NotEmpty();
        }
    }
}
