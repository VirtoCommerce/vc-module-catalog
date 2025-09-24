using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using System;

namespace VirtoCommerce.CatalogModule.Data.Validation;

public class ProductValidator : AbstractValidator<CatalogProduct>
{
    private static readonly System.Buffers.SearchValues<char> IllegalCodeChars = System.Buffers.SearchValues.Create("$+;=%{}[]|@~!^*&()<>\r\n");

    public ProductValidator(AbstractValidator<Property> propertyValidator)
    {
        RuleFor(product => product.CatalogId)
            .NotNull()
            .NotEmpty();

        RuleFor(product => product)
            .Must(product => (product.Id == null && product.MainProductId == null) || product.MainProductId != product.Id)
            .WithMessage("Self referencing loop detected.");

        RuleFor(product => product.Name)
            .NotNull().WithMessage(x => $"Name is null. Code: {x.Code}.")
            .NotEmpty().WithMessage(x => $"Name is empty. Code: {x.Code}.")
            .MaximumLength(1024)
            .DependentRules(() => RuleFor(product => product.Name)
                .Must(name => name == name.Trim() && !name.Any(char.IsControl)).WithMessage("Name must not contain leading/trailing spaces or control characters.")
                .Must(name => name.AsSpan().IndexOfAny(IllegalCodeChars) < 0).WithMessage("Product name contains illegal chars."));

        RuleFor(product => product.Code)
            .NotNull()
            .NotEmpty()
            .MaximumLength(64)
            .DependentRules(() => RuleFor(product => product.Code)
                .Must(code => code.AsSpan().IndexOfAny(IllegalCodeChars) < 0).WithMessage("Product code contains illegal chars."));

        // Validate custom product properties
        RuleForEach(product => product.Properties)
            .Where(x => !x.IsManageable)
            .SetValidator(propertyValidator);
    }
}

