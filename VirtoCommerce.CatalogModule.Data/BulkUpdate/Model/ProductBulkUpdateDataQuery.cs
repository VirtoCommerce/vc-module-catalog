namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class ProductBulkUpdateDataQuery : BulkUpdateDataQuery
    {
        public string[] CategoryIds { get; set; }
        public string[] CatalogIds { get; set; }
    }
}
