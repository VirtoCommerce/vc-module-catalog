using Ganss.Xss;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;
public class PropertyValueSanitizer(IHtmlSanitizer htmlSanitizer) : IPropertyValueSanitizer
{
    public virtual string Sanitize(string input)
    {
        return htmlSanitizer.Sanitize(input);
    }
}
