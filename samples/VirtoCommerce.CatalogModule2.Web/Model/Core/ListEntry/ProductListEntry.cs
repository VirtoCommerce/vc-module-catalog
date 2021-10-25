using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model.ListEntry
{
    /// <summary>
    /// Product ListEntry record.
    /// </summary>
    public class ProductListEntry2 : ProductListEntry
    {
        public override ListEntryBase FromModel(AuditableEntity entity)
        {
            return base.FromModel(entity);
        }
    }
}
