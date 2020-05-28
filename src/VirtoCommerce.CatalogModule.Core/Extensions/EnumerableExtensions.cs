using System;
using System.Collections.Generic;
using System.Text;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{

    public static class EnumerablePaginationExtensions
    {
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> items, int pageSize)
        {
            var page = new List<T>();
            foreach (var item in items)
            {
                page.Add(item);
                if (page.Count >= pageSize)
                {
                    yield return page;
                    page = new List<T>();
                }
            }
            if (page.Count > 0)
            {
                yield return page;
            }
        }
    }

}
