using System;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Extensions;

public static class EnumConvertExtension
{
    public static IndexDocumentChangeType ToIndexDocumentChangeType(this EntryState entryState)
    {
        return entryState switch
        {
            EntryState.Added => IndexDocumentChangeType.Created,
            EntryState.Deleted => IndexDocumentChangeType.Deleted,
            EntryState.Modified => IndexDocumentChangeType.Modified,
            _ => throw new ArgumentOutOfRangeException(nameof(entryState), entryState, null)
        };
    }
}

