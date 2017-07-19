using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class ProductValidator : AbstractValidator<CatalogProduct>
    {
        private static char[] _illegalCodeChars = new[] { '$', '+', ';', '=', '%', '{', '}', '[', ']', '|', '\\', '/', '@', '~', '!', '^', '*', '&', '(', ')', ':', '<', '>' };
        public ProductValidator()
        {
            
            RuleFor(product => product.CatalogId).NotNull();
            RuleFor(product => product.Name).NotNull().MaximumLength(1024);
            RuleFor(product => product.Code).NotNull().MaximumLength(64);
            RuleFor(product => product.Code).Must(x => x.IndexOfAny(_illegalCodeChars) < 0).WithMessage("product code contains illegal chars");
        }
    }
}
