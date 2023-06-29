using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IPropertyUpdateManager
    {
        IList<Property> GetStandardProperties();
        bool TryChangeProductPropertyValues(CatalogProduct product, Property[] properties);

        Task<ChangeProductPropertiesResult> TryChangeProductPropertyValues(CatalogProduct product, JObject source, string language);
    }
}
