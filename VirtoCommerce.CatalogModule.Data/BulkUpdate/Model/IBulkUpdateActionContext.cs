namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IBulkUpdateActionContext
    {
        string ActionName { get; set; }
        BulkUpdateDataQuery DataQuery { get; set; }
    }
}
