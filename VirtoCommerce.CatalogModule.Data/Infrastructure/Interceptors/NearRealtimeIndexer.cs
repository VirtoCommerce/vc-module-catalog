using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogModule.Data.Infrastructure.Interceptors
{
    public class NearRealtimeIndexer : IIndexingInterceptor
    {
        private ISearchProvider _searchProvider;
        private readonly Func<ISearchProvider> _searchProviderFactory;
        private readonly ISettingsManager _settingsManager;
        private IIndexingManager _indexingManager;
        private readonly Func<IIndexingManager> _indexingManagerFactory;
        private readonly string[] _entityTypes = { typeof(ItemEntity).Name, typeof(CategoryEntity).Name, "PriceEntity" };

        private ISearchProvider SearchProvider => _searchProvider ?? (_searchProvider = _searchProviderFactory());
        private IIndexingManager IndexingManager => _indexingManager ?? (_indexingManager = _indexingManagerFactory());

        public NearRealtimeIndexer(Func<ISearchProvider> searchProviderFactory, ISettingsManager settingsManager, Func<IIndexingManager> indexingManagerFactory)
        {
            if (searchProviderFactory == null) throw new ArgumentNullException(nameof(searchProviderFactory));
            if (settingsManager == null) throw new ArgumentNullException(nameof(settingsManager));
            if (indexingManagerFactory == null) throw new ArgumentNullException(nameof(indexingManagerFactory));

            _searchProviderFactory = searchProviderFactory;
            _settingsManager = settingsManager;
            _indexingManagerFactory = indexingManagerFactory;
        }

        public void Before(InterceptionContext context)
        {
        }

        public void After(InterceptionContext context)
        {
            var autoIndexing = _settingsManager.GetValue("VirtoCommerce.Search.AutoIndexing.Enable", false);
            var scheduleJobs = _settingsManager.GetValue("VirtoCommerce.Search.IndexingJobs.Enable", true);
            if (autoIndexing && !scheduleJobs)
            {
                ProcessEntriesPerState(context.EntriesByState);
            }
        }

        private void ProcessEntriesPerState(ILookup<EntityState, DbEntityEntry> entriesPerState)
        {
            foreach (var entriesWithState in entriesPerState)
            {
                if (entriesWithState.Key.Equals(EntityState.Unchanged)) continue;
                
                var entriesPerType = entriesWithState.GroupBy(x => GetSuitableEntityType(x.Entity.GetType()));

                foreach (var entries in entriesPerType.Where(x => x.Key != null))
                {
                    if (!MapEntityTypeToDocumentType(entries.Key, out string documentType)) continue;

                    var ids = entries.Select(x => ((Entity) x.Entity).Id);
                    if (entriesWithState.Key.Equals(EntityState.Deleted))
                    {
                        SearchProvider.RemoveAsync(documentType, ids.Select(x => new IndexDocument(x)).ToList());
                    }
                    else
                    {
                        var options = new IndexingOptions
                        {
                            BatchSize = _settingsManager.GetValue("VirtoCommerce.Search.IndexPartitionSize", 50),
                            DocumentIds = ids.ToList(),
                            DocumentType = documentType
                        };

                        IndexingManager.IndexAsync(options, null, new CancellationToken(false));
                    }
                }
            }
        }

        private Type GetSuitableEntityType(Type entityType)
        {
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.BaseType;
            }
            //This line allows you to use the base types to check that the current object type is matches the specified patterns
            var inheritanceChain = entityType.GetTypeInheritanceChainTo(typeof(Entity));
            return inheritanceChain.FirstOrDefault(x => IsMatchInExpression(_entityTypes, x.Name));
        }

        private bool MapEntityTypeToDocumentType(Type tpye, out string documentType)
        {
            if (tpye == typeof(ItemEntity))
            {
                documentType = KnownDocumentTypes.Product;
                return true;
            }
            if (tpye == typeof(CategoryEntity))
            {
                documentType = KnownDocumentTypes.Category;
                return true;
            }
            documentType = null;
            return false;
        }

        private bool IsMatchInExpression(string[] expressions, string name)
        {
            var retVal = true;

            if (expressions != null)
            {
                var inverse = expressions.Any(x => x.Contains("!"));
                expressions = expressions.Select(x => x.Replace("!", "")).ToArray();

                if (!string.IsNullOrEmpty(name))
                {
                    retVal = expressions.Any(x => string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase));
                    retVal = inverse ? !retVal : retVal;
                }
            }

            return retVal;
        }
    }
}
