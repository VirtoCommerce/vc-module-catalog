namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public abstract class BulkUpdateActionContext
    {
        public string ActionName { get; set; }
        public BulkUpdateDataQuery DataQuery { get; set; }
    }
}
