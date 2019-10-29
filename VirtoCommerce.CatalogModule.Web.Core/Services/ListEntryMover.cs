using System.Collections.Generic;
using System.Linq;

using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public class ListEntryMover<T>
        where T : class, IEntity
    {
        public virtual void ConfirmMove(IEnumerable<T> entities)
        {
        }

        public virtual List<T> PrepareMove(MoveInfo moveInfo)
        {
            return Enumerable.Empty<T>().ToList();
        }
    }
}
