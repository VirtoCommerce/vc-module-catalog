using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CatalogLanguage : Entity
    {

        [StringLength(64)]
        public string Language { get; set; }

        #region Navigation Properties
        public string CatalogId { get; set; }
        public virtual Catalog Catalog { get; set; }
        #endregion
    }
}
