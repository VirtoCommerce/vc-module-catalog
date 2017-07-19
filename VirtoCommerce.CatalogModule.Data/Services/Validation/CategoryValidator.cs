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
            RuleFor(category => category.CatalogId).NotNull();
            RuleFor(category => category.Code).NotNull().MaximumLength(64);
            RuleFor(category => category.Name).NotNull().MaximumLength(128);
        }
    }
}
