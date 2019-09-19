using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesActionContext : BulkUpdateActionContext
    {
        public Property[] Properties { get; set; }
        public ListEntryDataQuery DataQuery { get; set; }
    }
}
