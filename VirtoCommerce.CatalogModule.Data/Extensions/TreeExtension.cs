using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class TreeExtension
    {
        public static IEnumerable<TItem> GetAncestors<TItem>(this TItem item, Func<TItem, TItem> getParentFunc)
        {
            if (getParentFunc == null)
            {
                throw new ArgumentNullException(nameof(getParentFunc));
            }
            if (ReferenceEquals(item, null)) yield break;
            for (TItem curItem = getParentFunc(item); !ReferenceEquals(curItem, null); curItem = getParentFunc(curItem))
            {
                yield return curItem;
            }
        }
    }
}
