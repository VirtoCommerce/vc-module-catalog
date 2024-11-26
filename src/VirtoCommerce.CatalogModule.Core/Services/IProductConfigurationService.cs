using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IProductConfigurationService : ICrudService<ProductConfiguration>
{
    Task SaveChangesAsync(ProductConfiguration configuration, CancellationToken cancellationToken);
}
