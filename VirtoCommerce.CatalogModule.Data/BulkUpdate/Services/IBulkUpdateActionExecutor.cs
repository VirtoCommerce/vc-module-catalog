using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public interface IBulkUpdateActionExecutor
    {
        void Execute(BulkUpdateActionContext context, Action<BulkUpdateProgressInfo> progressCallback, ICancellationToken token);
    }
}