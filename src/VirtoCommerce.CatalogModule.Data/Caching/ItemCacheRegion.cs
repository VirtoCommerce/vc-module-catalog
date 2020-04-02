using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CatalogModule.Data.Caching
{
    public class ItemCacheRegion : CancellableCacheRegion<ItemCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _entityRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static IChangeToken CreateChangeToken(CatalogProduct[] products)
        {
            if (products == null)
            {
                throw new ArgumentNullException(nameof(products));
            }
            return CreateChangeToken(products.Select(x => x.Id).ToArray());
        }

        public static IChangeToken CreateChangeToken(string[] productIds)
        {
            if (productIds == null)
            {
                throw new ArgumentNullException(nameof(productIds));
            }
            var changeTokens = new List<IChangeToken>() { CreateChangeToken() };
            foreach (var productId in productIds)
            {
                changeTokens.Add(new CancellationChangeToken(_entityRegionTokenLookup.GetOrAdd(productId, new CancellationTokenSource()).Token));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireEntity(CatalogProduct entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (_entityRegionTokenLookup.TryRemove(entity.Id, out var token))
            {
                token.Cancel();
            }
            //need to also evict from cache a main product if given product is variation
            if (entity.MainProductId != null && _entityRegionTokenLookup.TryRemove(entity.MainProductId, out var token2))
            {
                token2.Cancel();
            }
        }
    }    
}
