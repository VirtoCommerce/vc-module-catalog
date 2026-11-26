using System.Collections.Concurrent;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport;

// A solution's CatalogSeoInfoDefaults that does what the base does and notes the catalogs it is asked about, so a test
// that installs it disturbs no other test.
public class RecordingSeoInfoDefaults : CatalogSeoInfoDefaults
{
    public static ConcurrentBag<string> CatalogIds { get; } = [];

    public override void EnsureSeoInfo(Catalog catalog)
    {
        CatalogIds.Add(catalog.Id);
        base.EnsureSeoInfo(catalog);
    }
}
