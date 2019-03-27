using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class SortInfoExtension
    {
        public static SortInfo[] EnsureIdIncluded(this SortInfo[] sortInfos)
        {
            if (!sortInfos.IsNullOrEmpty())
            {
                if (!sortInfos.Any(x => x.SortColumn.EqualsInvariant("id")))
                {
                    var sortInfoLst = sortInfos.ToList();
                    sortInfoLst.Add(new SortInfo()
                    {
                        SortColumn = "id"
                    });
                    return sortInfoLst.ToArray();
                }
            }
            return sortInfos;
        }
    }
}
