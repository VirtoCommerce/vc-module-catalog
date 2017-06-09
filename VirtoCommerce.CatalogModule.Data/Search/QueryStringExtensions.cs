using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public static class QueryStringExtensions
    {
        public static List<StringKeyValues> AsKeyValues(this string[] query)
        {
            var result = new List<StringKeyValues>();

            if (query != null)
            {
                var nameValueDelimeter = new[] { ':' };
                var valuesDelimeter = new[] { ',' };

                result.AddRange(query
                    .Select(item => item.Split(nameValueDelimeter, 2))
                    .Where(item => item.Length == 2)
                    .Select(item => new StringKeyValues { Key = item[0], Values = item[1].Split(valuesDelimeter, StringSplitOptions.RemoveEmptyEntries) })
                    .GroupBy(item => item.Key)
                    .Select(g => new StringKeyValues { Key = g.Key, Values = g.SelectMany(i => i.Values).Distinct().ToArray() })
                    );
            }

            return result;
        }

        /// <summary>
        /// Parses strings of form "sort=name desc,price asc" and so on.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static SortInfo[] AsSortInfoes(this string[] query)
        {
            var result = new List<SortInfo>();

            if (query != null && query.Length > 0)
            {
                var directionDelimeter = new[] { '-' };

                result.AddRange(query
                        .Select(item => item.Split(directionDelimeter, 2))
                        .Select(item => new SortInfo { SortColumn = item[0], SortDirection = item.Length == 1 ? SortDirection.Ascending : item[1] == "descending" || item[1] == "desc" ? SortDirection.Descending : SortDirection.Ascending })
                    );
            }

            return result.ToArray();
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

        public static string AsCatalog(this string outline)
        {
            if (string.IsNullOrEmpty(outline))
            {
                return string.Empty;
            }

            var outlineArray = outline.Split('/');

            return outlineArray.Length > 0 ? outlineArray[0] : string.Empty;
        }
    }

    public class StringKeyValues
    {
        public string Key { get; set; }
        public string[] Values { get; set; }
    }
}
