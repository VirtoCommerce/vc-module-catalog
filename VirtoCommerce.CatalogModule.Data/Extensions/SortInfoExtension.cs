using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class SortInfoExtension
    {
        public static IOrderedQueryable<T> OrderBySortInfosWithId<T>(this IQueryable<T> query, SortInfo[] sortInfos) where T : Entity
        {
            return query.OrderBySortInfos(sortInfos.EnsureIdIncluded());
        }


        private static SortInfo[] EnsureIdIncluded(this SortInfo[] sortInfos)
        {
            var result = sortInfos;

            if (sortInfos?.Any(x => x.SortColumn.EqualsInvariant("id")) != true)
            {
                var list = sortInfos?.ToList() ?? new List<SortInfo>();
                list.Add(new SortInfo { SortColumn = "id" });
                result = list.ToArray();
            }

            return result;
        }
    }
}
