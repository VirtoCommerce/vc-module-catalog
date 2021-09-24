using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Caching
{
    public class ItemCacheRegion : CancellableCacheRegion<ItemCacheRegion>
    {
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
                changeTokens.Add(CreateChangeTokenForKey(productId));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireEntity(CatalogProduct entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            ExpireTokenForKey(entity.Id);

            //need to also evict from cache a main product if given product is variation
            if (entity.MainProductId != null)
            {
                ExpireTokenForKey(entity.MainProductId);
            }
        }

        public static void ExpireProducts(string[] productIds)
        {
            if (!productIds.IsNullOrEmpty())
            {
                foreach (var id in productIds)
                {
                    ExpireTokenForKey(id);
                }
            }
        }
    }
}
