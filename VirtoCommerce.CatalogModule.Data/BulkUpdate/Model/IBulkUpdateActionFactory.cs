namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IBulkUpdateActionFactory
    {
        IBulkUpdateAction Create(BulkUpdateActionContext context);
    }
}
