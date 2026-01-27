using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public static class IndexDocumentHelper
    {
        public const string ObjectFieldName = "__object";

        public static void AddObjectFieldValue<T>(this IndexDocument document, T value)
        {
            document.Add(new IndexDocumentField(ObjectFieldName, value, IndexDocumentFieldValueType.Complex) { IsRetrievable = false, IsFilterable = false, IsSearchable = false });
        }

        public static T GetObjectFieldValue<T>(this SearchDocument document)
            where T : class
        {
            if (!document.TryGetValue(ObjectFieldName, out var obj))
            {
                return null;
            }

            var objType = AbstractTypeFactory<T>.TryCreateInstance().GetType();
            var result = obj switch
            {
                T tObj => tObj,
                JObject jObj => jObj.ToObject(objType) as T,
                string sObj when !string.IsNullOrEmpty(sObj) => DeserializeObject(sObj, objType) as T,
                _ => null,
            };

            return result;
        }

        public static JsonSerializer ObjectSerializer { get; } = new()
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };

        public static string SerializeObject(object obj)
        {
            using var stringWriter = new StringWriter(new StringBuilder(256 /*default value from JsonConvert.SerializeObject*/), CultureInfo.InvariantCulture);
            using var jsonTextWriter = new JsonTextWriter(stringWriter);
            jsonTextWriter.Formatting = ObjectSerializer.Formatting;
            ObjectSerializer.Serialize(jsonTextWriter, obj, objectType: null);

            return stringWriter.ToString();
        }

        public static T DeserializeObject<T>(string str)
        {
            return (T)DeserializeObject(str, typeof(T));
        }

        public static object DeserializeObject(string str, Type type)
        {
            using var stringReader = new StringReader(str);
            using var jsonTextReader = new JsonTextReader(stringReader);
            var result = ObjectSerializer.Deserialize(jsonTextReader, type);

            return result;
        }
    }
}
