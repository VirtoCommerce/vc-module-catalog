using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.CatalogModule.Data.Search;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Web.JsonConverters
{
    public class SearchCriteriaJsonConverter : JsonConverter
    {        
        private readonly Type[] _knowTypes = new [] { typeof(ProductSearchCriteria), typeof(CategorySearchCriteria) };

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result = null;
            if (typeof(CategorySearchCriteria).IsAssignableFrom(objectType))
            {
                result = AbstractTypeFactory<CategorySearchCriteria>.TryCreateInstance();
            }
            if (typeof(ProductSearchCriteria).IsAssignableFrom(objectType))
            {
                result = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
            }
            var obj = JObject.Load(reader);
            serializer.Populate(obj.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
