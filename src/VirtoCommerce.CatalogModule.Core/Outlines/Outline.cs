using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Outlines;

/// <summary>
/// Represents the path from the catalog to one of the child objects (product or category):
/// catalog/parent-category1/.../parent-categoryN/object
/// </summary>
public class Outline : ICloneable
{
    /// <summary>
    /// Outline parts
    /// </summary>
    public ICollection<OutlineItem> Items { get; set; }

    public override string ToString()
    {
        return Items == null ? null : string.Join(OutlineString.ItemDelimiter, Items);
    }

    public object Clone()
    {
        var result = (Outline)MemberwiseClone();
        result.Items = Items?.Select(x => x.CloneTyped()).ToList();
        return result;
    }
}

