using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IPropertyService
    {
        [Obsolete(@"Need to remove after inheriting IPropertyService from ICrudService<Property>.")]
        Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<string> ids);

        Task<IEnumerable<Property>> GetAllCatalogPropertiesAsync(string catalogId);
        [Obsolete(@"Need to remove after inheriting IPropertyService from ICrudService<Property>.")]
        Task SaveChangesAsync(IEnumerable<Property> properties);
        [Obsolete(@"Need to remove after inheriting IPropertyService from ICrudService<Property>.")]
        Task DeleteAsync(IEnumerable<string> ids, bool doDeleteValues = false);
    }
}
