using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;

namespace VirtoCommerce.CatalogModule.BulkActions.Models
{
    public class BaseBulkActionContext : BulkActionContext
    {
        public DataQuery DataQuery { get; set; }
    }
}
