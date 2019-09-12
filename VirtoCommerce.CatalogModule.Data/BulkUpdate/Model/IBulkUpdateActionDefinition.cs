namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public interface IBulkUpdateActionDefinition
    {
        string Name { get; set; }

        string Description { get; set; }

        string IconUrl { get; set; }

        /// <summary>
        /// Entity types to which action could be applied: Category, Product, … 
        /// </summary>
        string[] AppliableTypes { get; set; }

        IBulkUpdateActionFactory Factory { get; set; }
        IPagedDataSourceFactory DataSourceFactory { get; set; }
    }
}
