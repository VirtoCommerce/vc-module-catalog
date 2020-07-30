using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.DataSources
{
    public class BaseDataSource : IDataSource
    {
        private readonly DataQuery _dataQuery;

        private readonly IListEntrySearchService _listEntrySearchService;

        private readonly int _pageSize = 50;

        private int _pageNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataSource"/> class.
        /// </summary>
        /// <param name="listEntrySearchService">
        /// The search service.
        /// </param>
        /// <param name="dataQuery">
        /// The data query.
        /// </param>
        public BaseDataSource(IListEntrySearchService listEntrySearchService, DataQuery dataQuery)
        {
            _listEntrySearchService = listEntrySearchService;
            _dataQuery = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery));
        }

        public IEnumerable<IEntity> Items { get; protected set; }

        public async Task<bool> FetchAsync()
        {
            if (_dataQuery.ListEntries.IsNullOrEmpty())
            {
                if (_dataQuery.SearchCriteria == null)
                {
                    Items = Array.Empty<IEntity>();
                }
                else
                {
                    var searchCriteria = BuildSearchCriteria(_dataQuery);
                    var searchResult = await _listEntrySearchService.SearchAsync(searchCriteria);
                    Items = searchResult.ListEntries;
                }
            }
            else
            {
                var skip = GetSkip();
                var take = GetTake();
                var entities = await GetNextItemsAsync(_dataQuery.ListEntries, skip, take);
                Items = entities.ToArray();
            }

            _pageNumber++;

            return Items.Any();
        }

        public virtual async Task<int> GetTotalCountAsync()
        {
            var result = 0;

            if (_dataQuery.ListEntries.IsNullOrEmpty())
            {
                if (_dataQuery.SearchCriteria == null)
                {
                    // idle
                }
                else
                {
                    var searchCriteria = BuildSearchCriteria(_dataQuery);
                    searchCriteria.Skip = 0;
                    searchCriteria.Take = 0;
                    var searchResult = await _listEntrySearchService.SearchAsync(searchCriteria);
                    result = searchResult.TotalCount;
                }
            }
            else
            {
                result = await GetEntitiesCountAsync(_dataQuery.ListEntries);
            }

            return result;
        }

        protected virtual CatalogListEntrySearchCriteria BuildSearchCriteria(DataQuery dataQuery)
        {
            if (dataQuery.SearchCriteria == null)
            {
                dataQuery.SearchCriteria = AbstractTypeFactory<CatalogListEntrySearchCriteria>.TryCreateInstance();
            }

            var result = dataQuery.SearchCriteria;

            result.Skip = GetSkip();
            result.Take = GetTake();
            result.WithHidden = true;
            result.ResponseGroup = !string.IsNullOrEmpty(result.ResponseGroup) ? result.ResponseGroup : CategoryResponseGroup.Full.ToString();
            if (string.IsNullOrEmpty(result.Keyword))
            {
                return result;
            }

            // need search in children categories if user specify keyword
            result.SearchInChildren = true;
            result.SearchInVariations = true;
            return result;
        }

        protected virtual Task<int> GetEntitiesCountAsync(ListEntryBase[] entries)
        {
            return Task.FromResult(entries.Count());
        }

        protected virtual Task<IEnumerable<IEntity>> GetNextItemsAsync(ListEntryBase[] entries, int skip,
            int take)
        {
            return Task.FromResult(entries.Skip(skip).Take(take).OfType<IEntity>());
        }

        private int GetSkip()
        {
            var skip = _dataQuery.Skip.GetValueOrDefault();
            return (_pageNumber * _pageSize) + skip;
        }

        private int GetTake()
        {
            return _dataQuery.Take.GetValueOrDefault(_pageSize);
        }
    }
}
