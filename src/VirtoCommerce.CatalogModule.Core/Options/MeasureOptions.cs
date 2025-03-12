namespace VirtoCommerce.CatalogModule.Core.Options;
public class MeasureOptions
{
    public const string SectionName = "Measures";

    /// <summary>
    /// URL or FS path of the default source for measures (default is Modules/$(VirtoCommerce.Catalog)/Content/measures.json)
    /// </summary>
    public string DefaultSource { get; set; } = "Modules/VirtoCommerce.Catalog/Content/measures.json";
}
