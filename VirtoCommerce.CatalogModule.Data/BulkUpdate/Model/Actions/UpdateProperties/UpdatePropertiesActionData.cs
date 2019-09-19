using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesActionData : IBulkUpdateActionData
    {
        public Property[] Properties { get; set; }
    }
}
