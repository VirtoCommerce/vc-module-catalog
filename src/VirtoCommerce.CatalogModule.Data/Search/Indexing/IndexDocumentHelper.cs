using System;
using System.IO;
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
            document.Add(new IndexDocumentField(ObjectFieldName, value) { IsRetrievable = false, IsFilterable = false, IsSearchable = false });
        }

        public static T GetObjectFieldValue<T>(this SearchDocument document)
            where T : class
        {
            T result = null;

            if (document.ContainsKey(ObjectFieldName))
            {
                var obj = document[ObjectFieldName];
                var objType = AbstractTypeFactory<T>.TryCreateInstance().GetType();
                result = obj as T;
                if (result == null)
                {
                    if (obj is JObject jobj)
                    {
                        result = jobj.ToObject(objType) as T;
                    }
                    else
                    {
                        var productString = obj as string;
                        if (!string.IsNullOrEmpty(productString))
                        {
                            result = DeserializeObject(productString, objType) as T;
                        }
                    }
                }
            }

            return result;
        }

        public static JsonSerializer ObjectSerializer { get; } = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };


        public static string SerializeObject(object obj)
        {
            using (var memStream = new MemoryStream())
            {
                obj.SerializeJson(memStream, ObjectSerializer);
                memStream.Seek(0, SeekOrigin.Begin);

                var result = memStream.ReadToString();
                return result;
            }
        }

        public static T DeserializeObject<T>(string str)
        {
            return (T)DeserializeObject(str, typeof(T));
        }

        public static object DeserializeObject(string str, Type type)
        {
            using (var stringReader = new StringReader(str))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                var result = ObjectSerializer.Deserialize(jsonTextReader, type);
                return result;
            }
        }
    }
}
