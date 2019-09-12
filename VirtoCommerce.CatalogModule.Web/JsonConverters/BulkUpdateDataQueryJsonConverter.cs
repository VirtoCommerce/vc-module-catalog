using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.JsonConverters
{
    public class BulkUpdateDataQueryJsonConverter : JsonConverter
    {
        private static readonly Type[] _knownTypes = { typeof(BulkUpdateDataQuery) };

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return _knownTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var typeName = objectType.Name;
            var dataQueryTypeName = obj["dataQueryType"];
            if (dataQueryTypeName != null)
            {
                typeName = dataQueryTypeName.Value<string>();
            }

            var result = AbstractTypeFactory<BulkUpdateDataQuery>.TryCreateInstance(typeName);
            if (result == null)
            {
                throw new NotSupportedException("Unknown BulkUpdateDataQuery type: " + typeName);
            }

            serializer.Populate(obj.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
