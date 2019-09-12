namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public abstract class BulkUpdateDataQuery
    {
        public string[] ObjectsIds { get; set; }
        public string Keyword { get; set; }
    }
}
