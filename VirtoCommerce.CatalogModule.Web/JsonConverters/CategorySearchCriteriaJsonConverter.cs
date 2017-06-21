using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.JsonConverters
{
    public class CategorySearchCriteriaJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CategorySearchCriteria);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result = null;

            if (objectType == typeof(CategorySearchCriteria))
            {
                result = AbstractTypeFactory<CategorySearchCriteria>.TryCreateInstance();
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
