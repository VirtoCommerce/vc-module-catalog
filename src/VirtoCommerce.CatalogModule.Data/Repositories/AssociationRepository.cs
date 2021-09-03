using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class AssociationRepository : DbContextRepositoryBase<CatalogDbContext>, IAssociationRepository
    {
        public AssociationRepository(CatalogDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<AssociationEntity> Associations => DbContext.Set<AssociationEntity>();

        public async Task<IEnumerable<AssociationEntity>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null)
        {
            return await Associations.Where(x => ids.Contains(x.Id))
                                            .ToArrayAsync();
        }
    }
}
