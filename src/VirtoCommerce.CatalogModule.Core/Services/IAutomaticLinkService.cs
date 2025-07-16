using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IAutomaticLinkService
{
    Task UpdateLinks(string categoryId, ICancellationToken cancellationToken);
}
