using Ganss.Xss;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;
public class SanitizerService(IHtmlSanitizer htmlSanitizer) : ISanitizerService
{
    public virtual string Sanitize(string input)
    {
        return htmlSanitizer.Sanitize(input);
    }
}
