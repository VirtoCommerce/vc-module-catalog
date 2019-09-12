namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class ActionContext : IBulkUpdateActionContext
    {
        public string ActionName { get; set; }
        public BulkUpdateDataQuery DataQuery { get; set; }
        public string CategoryId { get; set; }
        public string CatalogId { get; set; }
    }
}
