using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    class PropertyValuesMapper
    {
        private readonly IPropertyDictionaryItemSearchService _propertyDictionarySearchService;

        private readonly List<Property> _properties = new();
        private readonly List<string> _dictionariesIds = new();

        public PropertyValuesMapper(IPropertyDictionaryItemSearchService propertyDictionarySearchService)
        {
            _propertyDictionarySearchService = propertyDictionarySearchService;
        }

        public void AddProperty(Property property)
        {
            _properties.Add(property.CloneTyped());
            if (property.Dictionary && !_dictionariesIds.Contains(property.Id))
            {
                _dictionariesIds.Add(property.Id);
            }
        }

        public async Task<List<Property>> GetResultProperties(CatalogProduct product, JObject source, string language)
        {
            var dictionaries = await GetDictionaryItems();

            foreach (var property in _properties)
            {
                var value = source[property.Name];

                var context = new PropertyValueProcessor.Context
                {
                    Property = property,
                    Value = value,
                    DictionaryItems = dictionaries,
                    Language = language
                };

                var strategy = PropertyValueProcessor.Create(context);

                strategy.Process();
            }

            return _properties;
        }

        private async Task<IEnumerable<PropertyDictionaryItem>> GetDictionaryItems()
        {
            if (!_dictionariesIds.Any())
            {
                return Array.Empty<PropertyDictionaryItem>();
            }

            var criteria = new PropertyDictionaryItemSearchCriteria { PropertyIds = _dictionariesIds, Take = 1000 };

            var dictionaries = await _propertyDictionarySearchService.SearchAsync(criteria);
            return dictionaries.Results;
        }

    }
}
