using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrThrow<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, string exceptionMessage)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException(exceptionMessage);
            }
            return value;
        }
    }
}
