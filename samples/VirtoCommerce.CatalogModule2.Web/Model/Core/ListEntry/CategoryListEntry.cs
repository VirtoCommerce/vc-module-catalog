using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model.ListEntry
{
    /// <summary>
    /// Category ListEntry record.
    /// </summary>
    public class CategoryListEntry2 : CategoryListEntry
    {
        public override ListEntryBase FromModel(AuditableEntity entity)
        {
            return base.FromModel(entity);
        }
    }
}
