using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IPagedDataSource
    {
        int PageSize { get; set; }
        bool Fetch();
        IEnumerable<IEntity> Items { get; }
        int GetTotalCount();
    }
}
