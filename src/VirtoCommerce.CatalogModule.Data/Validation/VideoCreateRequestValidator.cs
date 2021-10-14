using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class VideoCreateRequestValidator : AbstractValidator<VideoCreateRequest>
    {
        public VideoCreateRequestValidator()
        {
            RuleFor(x => x.ContentUrl).NotEmpty();
            RuleFor(x => x.OwnerId).NotEmpty();
            RuleFor(x => x.OwnerType).NotEmpty();
        }
    }
}
