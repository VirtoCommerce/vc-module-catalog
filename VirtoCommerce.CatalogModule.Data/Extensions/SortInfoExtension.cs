using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class SortInfoExtension
    {
        public static SortInfo[] EnsureIdIncluded(this SortInfo[] sortInfos)
        {
            var sortInfoList = sortInfos?.ToList() ?? new List<SortInfo>();
            if (!sortInfoList.Any(x => x.SortColumn.EqualsInvariant("id")))
            {
                sortInfoList.Add(new SortInfo
                {
                    SortColumn = "id"
                });
            }

            return sortInfoList.ToArray();
        }
    }
}
