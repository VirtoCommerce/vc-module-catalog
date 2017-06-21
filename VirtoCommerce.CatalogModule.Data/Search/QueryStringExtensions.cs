using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public static class QueryStringExtensions
    {
        public static IList<StringKeyValues> AsKeyValues(this IEnumerable<string> query)
        {
            var result = new List<StringKeyValues>();

            if (query != null)
            {
                var nameValueDelimeter = new[] { ':' };
                var valuesDelimeter = new[] { ',' };

                result.AddRange(query
                    .Select(item => item.Split(nameValueDelimeter, 2))
                    .Where(item => item.Length == 2)
                    .Select(item => new StringKeyValues { Key = item[0], Values = item[1].Split(valuesDelimeter, StringSplitOptions.RemoveEmptyEntries) }));
            }

            return result;
        }

        public static string AsCategoryId(this string outline)
        {
            if (string.IsNullOrEmpty(outline))
            {
                return string.Empty;
            }

            var outlineArray = outline.Split('/');

            return outlineArray.Length > 0 ? outlineArray[outlineArray.Length - 1] : string.Empty;
        }
    }

    public class StringKeyValues
    {
        public string Key { get; set; }
        public string[] Values { get; set; }
    }
}
