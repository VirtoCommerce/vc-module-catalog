using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyAttribute : AuditableEntity
    {
        [Required]
        [StringLength(128)]
        public string PropertyAttributeName { get; set; }

        [Required]
        [StringLength(128)]
        public string PropertyAttributeValue { get; set; }

        public int Priority { get; set; }

        #region Navigation Properties

        public string PropertyId { get; set; }

        public virtual Property Property { get; set; }

        #endregion
    }
}
