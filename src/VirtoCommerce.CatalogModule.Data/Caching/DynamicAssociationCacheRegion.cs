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
    public class DynamicAssociationCacheRegion : CancellableCacheRegion<DynamicAssociationCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _entityRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static IChangeToken CreateChangeToken(string[] dynamicAssociationIds)
        {
            if (dynamicAssociationIds == null)
            {
                throw new ArgumentNullException(nameof(dynamicAssociationIds));
            }

            var changeTokens = new List<IChangeToken> { CreateChangeToken() };

            changeTokens.AddRange(
                dynamicAssociationIds
                    .Select(dynamicAssocitationId => new CancellationChangeToken(_entityRegionTokenLookup.GetOrAdd(dynamicAssocitationId, new CancellationTokenSource()).Token)
                    )
                );

            return new CompositeChangeToken(changeTokens);
        }

        public static IChangeToken CreateChangeToken(DynamicAssociation[] dynamicAssociations)
        {
            if (dynamicAssociations == null)
            {
                throw new ArgumentNullException(nameof(dynamicAssociations));
            }

            return CreateChangeToken(dynamicAssociations.Select(x => x.Id).ToArray());
        }

        public static void ExpireEntity(DynamicAssociation dynamicAssociation)
        {
            if (dynamicAssociation == null)
            {
                throw new ArgumentNullException(nameof(dynamicAssociation));
            }

            if (_entityRegionTokenLookup.TryRemove(dynamicAssociation.Id, out var token))
            {
                token.Cancel();
            }
        }
    }
}
