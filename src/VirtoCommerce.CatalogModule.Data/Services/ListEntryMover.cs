using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ListEntryMover<T>
        where T : class, IEntity
    {
        public virtual Task ConfirmMoveAsync(IEnumerable<T> entities)
        {
            return Task.CompletedTask;
        }

        public virtual Task<List<T>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            return Task.FromResult(Enumerable.Empty<T>().ToList());
        }
    }
}
