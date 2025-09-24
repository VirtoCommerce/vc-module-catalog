using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class ProductValidator : AbstractValidator<CatalogProduct>
    {
        private static readonly char[] _illegalCodeChars = { '$', '+', ';', '=', '%', '{', '}', '[', ']', '|', '@', '~', '!', '^', '*', '&', '(', ')', '<', '>' };

        public ProductValidator(AbstractValidator<Property> propertyValidator)
        {
            RuleFor(product => product.CatalogId).NotNull().NotEmpty();
            RuleFor(product => product).Must(product => (product.Id == null && product.MainProductId == null) || product.MainProductId != product.Id).WithMessage("Self referencing loop detected");
            RuleFor(product => product.Name).NotNull().WithMessage(x => $"Name is null. Code: {x.Code}").NotEmpty().WithMessage(x => $"Name is empty. Code: {x.Code}").MaximumLength(1024);
            RuleFor(product => product.Code).NotNull().NotEmpty().MaximumLength(64).DependentRules(() => RuleFor(product => product.Code).Must(x => x.IndexOfAny(_illegalCodeChars) < 0).WithMessage("product code contains illegal chars"));

            // Validate custom product properties
            RuleForEach(product => product.Properties)
                .Where(x => !x.IsManageable)
                .SetValidator(propertyValidator);
        }
    }
}
