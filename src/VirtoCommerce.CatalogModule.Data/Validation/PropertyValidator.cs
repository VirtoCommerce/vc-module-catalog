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
        }
    }
}
