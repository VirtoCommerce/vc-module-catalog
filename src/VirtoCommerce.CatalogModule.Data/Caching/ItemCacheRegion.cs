using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Caching
{
    public class ItemCacheRegion : GenericCachingRegion<CatalogProduct>
    {
        // TODO: Remove. This method is for backward compatibility only
        public static IChangeToken CreateChangeToken(CatalogProduct[] products)
        {
            if (products == null)
            {
                throw new ArgumentNullException(nameof(products));
            }

            return CreateChangeToken((IEnumerable<CatalogProduct>)products);
        }

        // TODO: Remove. This method is for backward compatibility only
        public static IChangeToken CreateChangeToken(string[] productIds)
        {
            if (productIds == null)
            {
                throw new ArgumentNullException(nameof(productIds));
            }

            return CreateChangeToken((IEnumerable<string>)productIds);
        }

        public static void ExpireEntity(CatalogProduct entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            ExpireTokenForKey(entity.Id);

            // Need to also evict from cache a main product if given product is variation
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
