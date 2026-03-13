using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization for catalog objects (products, categories, etc.)
/// with settings optimized for compact storage (e.g. product snapshots in orders).
/// </summary>
public static class ProductJsonSerializer
{
    public static JsonSerializer ObjectSerializer { get; } = new()
    {
        DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
    };

    public static string Serialize(object obj)
    {
        using var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
        using var jsonTextWriter = new JsonTextWriter(stringWriter);
        jsonTextWriter.Formatting = ObjectSerializer.Formatting;
        ObjectSerializer.Serialize(jsonTextWriter, obj, objectType: null);

        return stringWriter.ToString();
    }

    public static T Deserialize<T>(string json)
    {
        return (T)Deserialize(json, typeof(T));
    }

    public static object Deserialize(string json, Type type)
    {
        using var stringReader = new StringReader(json);
        using var jsonTextReader = new JsonTextReader(stringReader);
        var result = ObjectSerializer.Deserialize(jsonTextReader, type);

        return result;
    }

    /// <summary>
    /// Deserializes JSON to the correct polymorphic type using <see cref="AbstractTypeFactory{T}"/>.
    /// </summary>
    public static T DeserializePolymorphic<T>(string json)
        where T : class
    {
        var type = AbstractTypeFactory<T>.TryCreateInstance().GetType();
        return Deserialize(json, type) as T;
    }
}
