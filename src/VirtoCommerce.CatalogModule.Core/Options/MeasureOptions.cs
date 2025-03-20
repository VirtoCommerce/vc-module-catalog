namespace VirtoCommerce.CatalogModule.Core.Options;
public class MeasureOptions
{
    public const string SectionName = "Measures";

    /// <summary>
    /// URL or FS path of the default source for measures
    /// </summary>
    public string DefaultSource { get; set; }
}
