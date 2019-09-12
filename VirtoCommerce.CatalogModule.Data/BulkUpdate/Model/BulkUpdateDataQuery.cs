namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class BulkUpdateDataQuery
    {
        public string[] ObjectsIds { get; set; }
        public string Keyword { get; set; }

        public string[] CategoryIds { get; set; }
        public string[] CatalogIds { get; set; }
    }
}
