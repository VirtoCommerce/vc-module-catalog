using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class AutomaticLinkQuery : AuditableEntity, ICloneable
{
    public string TargetCategoryId { get; set; }

    public string SourceCatalogId { get; set; }

    public string SourceCatalogQuery { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
