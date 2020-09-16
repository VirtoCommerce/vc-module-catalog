using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Patches target collection in memory and in repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="comparer"></param>
        /// <param name="patch"></param>
        /// <param name="repository"></param>
        public static void Patch<T>(this ICollection<T> source, ICollection<T> target, IEqualityComparer<T> comparer, Action<T, T> patch, IRepository repository) where T : class
        {
            Action<EntryState, T, T> patchAction = (state, x, y) =>
            {
                repository?.Attach(y);
                if (state == EntryState.Modified)
                {
                    patch(x, y);
                }
                else if (state == EntryState.Added)
                {
                    target.Add(x);
                    repository?.Add(y);
                }
                else if (state == EntryState.Deleted)
                {
                    target.Remove(x);
                    repository?.Remove(y);
                }
            };

            source.CompareTo(target, comparer, patchAction);
        }
    }
}
