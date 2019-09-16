namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory
{
    public class ChangeCategoryActionContext : BulkUpdateActionContext
    {
        public string CategoryId { get; set; }
        public string CatalogId { get; set; }
    }
}
