using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class PropertyValidator : AbstractValidator<Property>
    {
        public PropertyValidator()
        {

            //RuleFor(property => property.CatalogId).NotNull().NotEmpty();
            RuleFor(property => property.Name).NotNull().NotEmpty().WithMessage(x => $"Name is null or empty").MaximumLength(128);
            RuleFor(property => property.Dictionary).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);
            RuleFor(property => property.Multivalue).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);
            RuleFor(property => property.Multilanguage).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);
        }
    }
}
