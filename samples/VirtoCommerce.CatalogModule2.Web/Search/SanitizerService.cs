using Ganss.Xss;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class SanitizerService2(IHtmlSanitizer htmlSanitizer) : SanitizerService(htmlSanitizer)
    {
        public override string Sanitize(string html)
        {
            return base.Sanitize(html);
        }
    }
}
