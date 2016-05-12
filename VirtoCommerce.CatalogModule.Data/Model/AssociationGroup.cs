using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class AssociationGroup : AuditableEntity
    {
        public AssociationGroup()
        {
            Associations = new ObservableCollection<Association>();
        }

        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        public int Priority { get; set; }

        #region Navigation Properties

        public string ItemId { get; set; }
        public virtual Item CatalogItem { get; set; }

        public virtual ObservableCollection<Association> Associations { get; set; }
        #endregion
    }
}
