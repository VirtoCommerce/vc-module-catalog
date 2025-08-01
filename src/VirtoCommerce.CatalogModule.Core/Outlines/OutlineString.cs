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

    public static string GetLastItem(ReadOnlySpan<char> outline)
    {
        if (outline.IsEmpty)
        {
            return null;
        }

        var lastItemSpan = GetLastItemSpan(outline);

        return lastItemSpan.ToString();
    }

    public static string GetLastItemId(ReadOnlySpan<char> outline)
    {
        if (outline.IsEmpty)
        {
            return null;
        }

        var lastItem = GetLastItemSpan(outline);
        var nameDelimiterIndex = lastItem.LastIndexOf(NameDelimiter.AsSpan());

        var id = nameDelimiterIndex >= 0
            ? lastItem[..nameDelimiterIndex]
            : lastItem;

        return id.ToString();
    }


    private static ReadOnlySpan<char> GetLastItemSpan(ReadOnlySpan<char> outline)
    {
        var lastItemDelimiterIndex = outline.LastIndexOf(ItemDelimiter);

        var lastItem = lastItemDelimiterIndex >= 0
            ? outline[(lastItemDelimiterIndex + 1)..]
            : outline;

        return lastItem;
    }
}
