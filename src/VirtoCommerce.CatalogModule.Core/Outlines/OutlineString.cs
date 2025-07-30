using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Outlines;

// Outline format      : catalog-id/category1-id/.../categoryN-id/last-item_id
// Named outline format: catalog-id/category1-id/.../categoryN-id/last-item-id___last-item-name
public static class OutlineString
{
    public const char ItemDelimiter = '/';
    public const string NameDelimiter = "___";

    public static string Build(IEnumerable<string> ids, string lastItemName = null)
    {
        var outlineString = string.Join(ItemDelimiter, ids);

        return lastItemName.IsNullOrEmpty()
            ? outlineString
            : string.Join(NameDelimiter, outlineString, lastItemName);
    }

    public static string GetLastItemId(ReadOnlySpan<char> outlineString)
    {
        if (outlineString.IsEmpty)
        {
            return null;
        }

        var lastItemDelimiterIndex = outlineString.LastIndexOf(ItemDelimiter);

        var lastItemSpan = lastItemDelimiterIndex >= 0
            ? outlineString[(lastItemDelimiterIndex + 1)..]
            : outlineString;

        var nameDelimiterIndex = lastItemSpan.LastIndexOf(NameDelimiter.AsSpan());

        var idSpan = nameDelimiterIndex >= 0
            ? lastItemSpan[..nameDelimiterIndex]
            : lastItemSpan;

        return idSpan.ToString();
    }
}
