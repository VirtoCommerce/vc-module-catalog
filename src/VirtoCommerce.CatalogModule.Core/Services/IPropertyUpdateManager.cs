using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IPropertyUpdateManager
    {
        IList<Property> GetStandardProperties();
        bool TryChangeProductPropertyValues(CatalogProduct product, Property[] properties);

        Task TryChangeProductPropertyValues(CatalogProduct product, JObject source, string language, ModelStateDictionary modelState);
    }
}
