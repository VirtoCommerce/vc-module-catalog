namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IPagedDataSourceFactory
    {
        IPagedDataSource Create(BulkUpdateDataQuery dataQuery);
    }
}
