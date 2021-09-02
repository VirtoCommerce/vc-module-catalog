using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public interface IAssociationRepository : IRepository
    {
        IQueryable<AssociationEntity> Associations { get; }
        Task<IEnumerable<AssociationEntity>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null);
    }
}
