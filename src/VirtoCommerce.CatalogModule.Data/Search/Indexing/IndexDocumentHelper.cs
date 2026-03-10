using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Serialization;
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

            var result = obj switch
            {
                T tObj => tObj,
                JObject jObj => jObj.ToObject(AbstractTypeFactory<T>.TryCreateInstance().GetType()) as T,
                string sObj when !string.IsNullOrEmpty(sObj) => ProductJsonSerializer.DeserializePolymorphic<T>(sObj),
                _ => null,
            };

            return result;
        }

        public static JsonSerializer ObjectSerializer => ProductJsonSerializer.ObjectSerializer;

        public static string SerializeObject(object obj) => ProductJsonSerializer.Serialize(obj);

        public static T DeserializeObject<T>(string str) => ProductJsonSerializer.Deserialize<T>(str);

        public static object DeserializeObject(string str, Type type) => ProductJsonSerializer.Deserialize(str, type);
    }
}
