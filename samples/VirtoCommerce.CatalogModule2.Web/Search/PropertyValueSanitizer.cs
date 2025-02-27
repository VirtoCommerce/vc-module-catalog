using Ganss.Xss;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class PropertyValueSanitizer2(IHtmlSanitizer htmlSanitizer) : PropertyValueSanitizer(htmlSanitizer)
    {
        public override string Sanitize(string input)
        {
            return base.Sanitize(input);
        }
    }
}
