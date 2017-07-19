using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(category => category.CatalogId).NotNull().NotEmpty();
            RuleFor(category => category.Code).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(category => category.Name).NotNull().NotEmpty().MaximumLength(128);
        }
    }
}
