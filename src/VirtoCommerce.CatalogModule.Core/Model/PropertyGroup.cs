using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class PropertyGroup : AuditableEntity, IHasCatalogId, IExportable
    {
        public string Name { get; set; }

        public string CatalogId { get; set; }

        public LocalizedString LocalizedName { get; set; }

        public LocalizedString LocalizedDescription { get; set; }

        public int Priority { get; set; }

        public virtual object Clone()
        {
            var result = (PropertyGroup)MemberwiseClone();
            result.LocalizedName = LocalizedName?.CloneTyped();
            result.LocalizedDescription = LocalizedDescription?.CloneTyped();
            return result;
        }
    }
}
