using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CategoryItemRelation : Entity
    {
        public int Priority { get; set; }

        #region Navigation Properties
        public string ItemId { get; set; }
        public virtual Item CatalogItem { get; set; }

        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public string CatalogId { get; set; }
        public virtual Catalog Catalog { get; set; }

        #endregion
    }
}
