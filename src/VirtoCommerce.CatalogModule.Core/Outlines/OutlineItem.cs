using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
// todo: question, now, we have link to seo module here
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Outlines;

/// <summary>
/// Represents one outline element: catalog, category or product.
/// </summary>
public class OutlineItem : ISeoSupport, ICloneable
{
    #region ISeoSupport Members

    /// <summary>
    /// Object id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Object type
    /// </summary>
    public string SeoObjectType { get; set; }

    /// <summary>
    /// All SEO records for the object
    /// </summary>
    public IList<SeoInfo> SeoInfos { get; set; }

    #endregion
    /// <summary>
    /// The name of current item
    /// </summary>
    public string Name { get; set; }

    public LocalizedString LocalizedName { get; set; }

    /// <summary>
    /// True when this object is linked to the virtual parent.
    /// </summary>
    public bool HasVirtualParent { get; set; }

    public override string ToString()
    {
        return (HasVirtualParent ? "*" : "") + Id;
    }

    public object Clone()
    {
        var result = MemberwiseClone() as OutlineItem;
        result.SeoInfos = SeoInfos?.Select(x => x.Clone() as SeoInfo).ToList();
        result.LocalizedName = LocalizedName?.Clone() as LocalizedString;
        return result;
    }
}
