using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class CatalogIndexedSearchCriteria : SearchCriteriaBase
    {
        public string StoreId { get; set; }

        public string CatalogId { get; set; }

        public IList<string> CatalogIds { get; set; }

        /// <summary>
        /// CategoryId1/CategoryId2, no catalog should be included in the outline
        /// </summary>
        public string Outline { get; set; }

        /// <summary>
        /// CategoryId1/CategoryId2, no catalog should be included in the outline
        /// </summary>
        public IList<string> Outlines { get; set; }

        /// <summary>
        /// Term format: name:value1,value2
        /// </summary>
        public IList<string> Terms { get; set; }

        /// <summary>
        /// Assigned groups for current user. Data format: user_groups:value1,value2
        /// </summary>
        public IList<string> UserGroups { get; set; }

        /// <summary>
        /// Enable fuzzy search, i.e. allow to search color:white even if color:wihte actually passed to criteria
        /// </summary>
        public bool IsFuzzySearch { get; set; }

        /// <summary>
        /// Gets or sets the search provider specific raw search query; all other search criteria will be ignored
        /// </summary>
        public string RawQuery { get; set; }

        /// <summary>
        /// Allows to retrieve only a specific set of fields in the result hits
        /// </summary>
        public IList<string> IncludeFields { get; set; }

        [JsonProperty("searchPhrase")]
        public new string Keyword { get; set; }

        public virtual CatalogIndexedSearchCriteria FromListEntryCriteria(CatalogListEntrySearchCriteria listEntryCriteria)
        {
            if (listEntryCriteria == null)
            {
                throw new ArgumentNullException(nameof(listEntryCriteria));
            }

            Keyword = listEntryCriteria.Keyword;
            CatalogId = listEntryCriteria.CatalogId;
            CatalogIds = listEntryCriteria.CatalogIds;
            Outline = listEntryCriteria.CategoryId;
            Skip = listEntryCriteria.Skip;
            Take = listEntryCriteria.Take;
            Sort = listEntryCriteria.Sort;

            return this;
        }
    }
}
