using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IBulkUpdateAction
    {
        IBulkUpdateActionContext Context { get; }

        IBulkUpdateActionData GetActionData();
        BulkUpdateActionResult Validate();
        BulkUpdateActionResult Execute(IEnumerable<IEntity> entities);
    }
}
