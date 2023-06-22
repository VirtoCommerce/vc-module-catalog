using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    internal static class ConverterHelper
    {
        private static readonly IDictionary<Type, Func<JToken, object>> Converters = new Dictionary<Type, Func<JToken, object>>
        {
            { typeof(bool?), value => value.Value<bool?>() },
            { typeof(bool), value => value.Value<bool>() },
            { typeof(DateTime?), value => value.Value<DateTime?>() },
            { typeof(DateTime), value => value.Value<DateTime>() },
            { typeof(int?), value => value.Value<int?>() },
            { typeof(int), value => value.Value<int>() },
            { typeof(decimal), value => value.Value<decimal>() },
            { typeof(string), value => value.Value<string>() }
        };

        public static object Convert(PropertyInfo property, JToken value)
        {
            return Converters[property.PropertyType](value);
        }
    }
}
