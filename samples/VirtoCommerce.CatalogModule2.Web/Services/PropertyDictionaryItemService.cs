using System;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class PropertyDictionaryItemService2 : PropertyDictionaryItemService
    {
        public PropertyDictionaryItemService2(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache) : base(repositoryFactory, platformMemoryCache)
        {
        }
    }
}

