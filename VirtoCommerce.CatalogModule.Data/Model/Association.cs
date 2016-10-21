using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class Association : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the type of the association. 
        /// </summary>
        /// <value>
        /// The type of the association.
        /// </value>
        [StringLength(128)]
        [Required]
        public string AssociationType { get; set; }
 
        public int Priority { get; set; }

        public int? Quantity { get; set; }

        [StringLength(1024)]
        public string Tags { get; set; }

        #region Navigation Properties

        public string ItemId { get; set; }
        public virtual Item Item { get; set; }

        public string AssociatedItemId { get; set; }
        public virtual Item AssociatedItem { get; set; }

        public string AssociatedCategoryId { get; set; }
        public Category AssociatedCategory { get; set; }
        #endregion
    }
}
