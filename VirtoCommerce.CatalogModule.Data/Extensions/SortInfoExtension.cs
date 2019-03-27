using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class SortInfoExtension
    {
        public static SortInfo[] EnsureIdIncluded(this SortInfo[] sortInfos)
        {
            List<SortInfo> sortInfoList = null;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfoList = new List<SortInfo>();
            }

            else if (!sortInfos.Any(x => x.SortColumn.EqualsInvariant("id")))
            {
                sortInfoList = sortInfos.ToList();
            }

            if (sortInfoList != null)
            {
                sortInfoList.Add(new SortInfo
                {
                    SortColumn = "id"
                });
                sortInfos = sortInfoList.ToArray();
            }

            return sortInfos;
        }
    }
}
