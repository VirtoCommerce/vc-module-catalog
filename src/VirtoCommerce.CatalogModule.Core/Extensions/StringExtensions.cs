namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static partial class StringExtensions
{
    public static string SoftTruncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        value = value[..maxLength];

        var idx = value.LastIndexOf(' ');

        if (idx != -1)
        {
            return value[..idx].Trim();
        }

        return value;
    }
}
