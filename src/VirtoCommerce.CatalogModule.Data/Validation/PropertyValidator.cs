using System.Text.RegularExpressions;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class PropertyValidator : AbstractValidator<Property>
    {
        private readonly string _nameValidationMessage = "Property name {0} must start with a letter or number, and can contain only Latin letters, digits, underscore ('_'). Every underscore('_') character must be immediately preceded and followed by a letter or number.";

        public PropertyValidator()
        {
            RuleFor(property => property.Name)
                .NotNull().NotEmpty()
                .WithMessage(x => $"Name is null or empty")
                .MaximumLength(128)
                .Matches(@"^\b[0-9a-z]+(_[0-9a-z]+)*\b$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
                .WithMessage(property => string.Format(_nameValidationMessage, property.Name));

            RuleFor(property => property.Dictionary).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);
            RuleFor(property => property.Multivalue).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);
            RuleFor(property => property.Multilanguage).NotEqual(true).When(property => property.ValueType == PropertyValueType.Boolean);

            RuleFor(property => property.Dictionary).NotEqual(true).When(property => property.ValueType == PropertyValueType.Integer);

            RuleFor(property => property.Dictionary).NotEqual(true).When(property => property.ValueType == PropertyValueType.GeoPoint);
            RuleFor(property => property.Multilanguage).NotEqual(true).When(property => property.ValueType == PropertyValueType.GeoPoint);

            RuleFor(property => property.Dictionary).NotEqual(true).When(property => property.ValueType == PropertyValueType.DateTime);
            RuleFor(property => property.Multivalue).NotEqual(true).When(property => property.ValueType == PropertyValueType.DateTime);
            RuleFor(property => property.Multilanguage).NotEqual(true).When(property => property.ValueType == PropertyValueType.DateTime);
        }
    }
}
