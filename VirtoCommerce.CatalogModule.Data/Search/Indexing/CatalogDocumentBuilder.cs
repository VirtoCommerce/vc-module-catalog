using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public abstract class CatalogDocumentBuilder
    {
        private readonly ISettingsManager _settingsManager;

        protected CatalogDocumentBuilder(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        protected virtual bool StoreObjectsInIndex => _settingsManager.GetValue("Catalog.Search.UseFullObjectIndexStoring", true);

        protected virtual void IndexCustomProperties(IndexDocument document, ICollection<Property> properties, ICollection<PropertyValue> propertyValues)
        {
            foreach (var propValue in propertyValues.Where(x => x.Value != null))
            {
                var property = properties.FirstOrDefault(p => p.Name.EqualsInvariant(propValue.PropertyName) && p.ValueType == propValue.ValueType);

                var propertyName = propValue.PropertyName?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var isCollection = property?.Multivalue == true;

                    switch (propValue.ValueType)
                    {
                        case PropertyValueType.Boolean:
                        case PropertyValueType.DateTime:
                        case PropertyValueType.Number:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.LongText:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value.ToString().ToLowerInvariant()) { IsRetrievable = true, IsSearchable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.ShortText:
                            // Index alias when it is available instead of display value.
                            // Do not tokenize small values as they will be used for lookups and filters.
                            var alias = GetPropertyValueAlias(property, propValue);
                            var shortTextValue = !string.IsNullOrEmpty(alias) ? alias : propValue.Value.ToString();
                            document.Add(new IndexDocumentField(propertyName, shortTextValue) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                    }
                }

                // Add value to the searchable content field
                var contentField = string.Concat("__content", property != null && property.Multilanguage && !string.IsNullOrWhiteSpace(propValue.LanguageCode) ? "_" + propValue.LanguageCode.ToLowerInvariant() : string.Empty);

                switch (propValue.ValueType)
                {
                    case PropertyValueType.LongText:
                    case PropertyValueType.ShortText:
                        var stringValue = propValue.Value.ToString();

                        if (!string.IsNullOrWhiteSpace(stringValue)) // don't index empty values
                        {
                            document.Add(new IndexDocumentField(contentField, stringValue.ToLower()) { IsRetrievable = true, IsSearchable = true, IsCollection = true });
                        }

                        break;
                }
            }
        }

        protected virtual string GetPropertyValueAlias(Property property, PropertyValue propValue)
        {
            var dictionaryValueAlias = property?.DictionaryValues?.Where(v => v.Id.EqualsInvariant(propValue.ValueId)).Select(v => v.Alias).FirstOrDefault();
            var result = !string.IsNullOrEmpty(dictionaryValueAlias) ? dictionaryValueAlias : propValue.Alias;
            return result;
        }

        protected virtual string[] GetOutlineStrings(IEnumerable<Outline> outlines)
        {
            return outlines
                .SelectMany(ExpandOutline)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        protected virtual IEnumerable<string> ExpandOutline(Outline outline)
        {
            // Outline structure: catalog/category1/.../categoryN/current-item

            var items = outline.Items
                .Take(outline.Items.Count - 1) // Exclude last item, which is current item ID
                .Select(i => i.Id)
                .ToList();

            var result = new List<string>();

            // Add partial outline for each parent:
            // catalog/category1/category2
            // catalog/category1
            // catalog
            if (items.Count > 0)
            {
                for (var i = items.Count; i > 0; i--)
                {
                    result.Add(string.Join("/", items.Take(i)));
                }
            }

            // For each parent category create a separate outline: catalog/parent_category
            if (items.Count > 2)
            {
                var catalogId = items.First();

                result.AddRange(
                    items.Skip(1)
                        .Select(i => string.Join("/", catalogId, i)));
            }

            return result;
        }
    }
}
