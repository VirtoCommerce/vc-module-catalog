using System.Threading.Tasks;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IPropertyValidatorService
    {
        Task<ValidationResult> ValidateAsync(PropertyValidationRequest request);
    }
}
