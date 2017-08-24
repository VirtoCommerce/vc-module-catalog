using System;
using FluentValidation;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    /// <summary>
    /// Custom validator for PropertyValue object, dynamically creates validation rules by passed PropertyValidationRule
    /// </summary>
    [CLSCompliant(false)]
    public class PropertyValueValidator : AbstractValidator<PropertyValue>
    {
        public PropertyValueValidator(PropertyValidationRule rule)
        {
            AttachValidators(rule);
        }

        private void AttachValidators(PropertyValidationRule rule)
        {
            if (rule.CharCountMax.HasValue)
            {
                AddLenghtMaxValidationRule(rule);
            }

            if (rule.CharCountMin.HasValue)
            {
                AddLenghtMinValidationRule(rule);
            }

            if (!string.IsNullOrEmpty(rule.RegExp))
            {
                AddRegexValidationRule(rule);
            }
        }

        private void AddLenghtMaxValidationRule(PropertyValidationRule validationRule)
        {
            RuleFor(s => s.Value.ToString())
                .MaximumLength(validationRule.CharCountMax.Value).WithName(validationRule.Property.Name);
        }

        private void AddLenghtMinValidationRule(PropertyValidationRule validationRule)
        {
            RuleFor(s => s.Value.ToString())
                .MinimumLength(validationRule.CharCountMin.Value).WithName(validationRule.Property.Name);
        }

        private void AddRegexValidationRule(PropertyValidationRule validationRule)
        {
            RuleFor(s => s.Value.ToString()).Matches(validationRule.RegExp).WithName(validationRule.Property.Name);
        }
    }
}