using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.BulkActions.Models
{
    public class PropertiesUpdateBulkActionContext : BaseBulkActionContext
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public Property[] Properties { get; set; }
    }
}
