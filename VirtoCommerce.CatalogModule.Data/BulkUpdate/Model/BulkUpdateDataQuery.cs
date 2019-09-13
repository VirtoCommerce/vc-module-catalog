namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public abstract class BulkUpdateDataQuery
    {
        public string[] ObjectIds { get; set; }
        public string Keyword { get; set; }
        public string DataQueryType => GetType().Name;

        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
