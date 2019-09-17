using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public interface IListEntryMover<T> where T : class, IEntity
    {
        List<T> PrepareMove(MoveInfo moveInfo);
        void ConfirmMove(IEnumerable<T> entities);
    }
}
