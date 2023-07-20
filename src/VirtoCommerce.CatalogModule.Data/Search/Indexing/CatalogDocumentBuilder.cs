using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public abstract class CatalogDocumentBuilder
    {
        private readonly ISettingsManager _settingsManager;

        protected CatalogDocumentBuilder(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        protected virtual bool StoreObjectsInIndex => _settingsManager.GetValue<bool>(ModuleConstants.Settings.Search.UseFullObjectIndexStoring);

        protected virtual void IndexCustomProperties(IndexDocument document, ICollection<Property> properties, ICollection<PropertyType> contentPropertyTypes)
        {
            foreach (var property in properties.Where(x => x != null))
            {
                IndexCustomProperty(document, property, contentPropertyTypes);
            }
        }

        protected virtual void IndexCustomProperty(IndexDocument document, Property property, ICollection<PropertyType> contentPropertyTypes, bool addField = true)
        {
            var isCollection = property.Multivalue;

            // replace empty value for Boolean property with default 'False'
            if (property.ValueType == PropertyValueType.Boolean && property.Values.IsNullOrEmpty())
            {
                if (addField)
                {
                    document.Add(new IndexDocumentField(property.Name, false, IndexDocumentFieldValueType.Boolean) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                }

                return;
            }

            foreach (var propValue in property.Values?.Where(x => x.Value != null) ?? Array.Empty<PropertyValue>())
            {
                var propertyName = propValue.PropertyName?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(propertyName) && addField)
                {
                    switch (propValue.ValueType)
                    {
                        case PropertyValueType.Boolean:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value, IndexDocumentFieldValueType.Boolean) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.DateTime:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value, IndexDocumentFieldValueType.DateTime) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.Integer:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value, IndexDocumentFieldValueType.Integer) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.Number:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value, IndexDocumentFieldValueType.Double) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.LongText:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value.ToString().ToLowerInvariant(), IndexDocumentFieldValueType.String) { IsRetrievable = true, IsSearchable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.ShortText:
                            // Index alias when it is available instead of display value.
                            // Do not tokenize small values as they will be used for lookups and filters.
                            var shortTextValue = propValue.Alias ?? propValue.Value.ToString();
                            document.Add(new IndexDocumentField(propertyName, shortTextValue, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            if (property.Multilanguage && !string.IsNullOrEmpty(propValue.LanguageCode))
                            {
                                document.Add(new IndexDocumentField($"{propertyName}_{propValue.LanguageCode.ToLowerInvariant()}", shortTextValue, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            }
                            break;
                        case PropertyValueType.GeoPoint:
                            document.Add(new IndexDocumentField(propertyName, GeoPoint.TryParse((string)propValue.Value), IndexDocumentFieldValueType.GeoPoint) { IsRetrievable = true, IsSearchable = true, IsCollection = true });
                            break;
                    }
                }

                // Add value to the searchable content field if property type is unknown or if it is present in the provided list
                if (contentPropertyTypes == null || contentPropertyTypes.Contains(property.Type))
                {
                    var contentField = string.Concat("__content", property.Multilanguage && !string.IsNullOrWhiteSpace(propValue.LanguageCode) ? $"_{propValue.LanguageCode.ToLowerInvariant()}" : string.Empty);

                    switch (propValue.ValueType)
                    {
                        case PropertyValueType.LongText:
                        case PropertyValueType.ShortText:
                            var stringValue = propValue.Value.ToString();

                            if (!string.IsNullOrWhiteSpace(stringValue)) // don't index empty values
                            {
                                document.Add(new IndexDocumentField(contentField, stringValue.ToLower(), IndexDocumentFieldValueType.String) { IsRetrievable = true, IsSearchable = true, IsCollection = true });
                            }

                            break;
                    }
                }
            }
        }

        protected virtual string[] GetOutlineStrings(IEnumerable<Outline> outlines, bool getNameLatestItem = false)
        {
            return outlines
                .SelectMany(x => ExpandOutline(x, getNameLatestItem))
                .Distinct()
                .ToArray();
        }

        protected virtual IEnumerable<string> ExpandOutline(Outline outline, bool getNameLatestItem)
        {
            // Outline structure: catalog/category1/.../categoryN/current-item

            var items = outline.Items
                .Take(outline.Items.Count - 1) // Exclude last item, which is current item ID
                                               // VP-6151 Need to save outlines in index in lower case as we are converting search criteria outlines to lower case
                .Select(i => i.Id.ToLowerInvariant())
                .ToList();

            var itemNames = outline.Items
                .Take(outline.Items.Count - 1) // Exclude last item, which is current item ID
                .ToDictionary(x => x.Id.ToLowerInvariant(), y => y.Name);

            var result = new List<string>();

            // Add partial outline for each parent:
            // catalog/category1/category2
            // catalog/category1
            // catalog
            if (items.Count > 0)
            {
                for (var i = items.Count; i > 0; i--)
                {
                    var path = !getNameLatestItem ? string.Join("/", items.Take(i)) :
                        string.Join(ModuleConstants.OutlineDelimiter,
                            string.Join("/", items.Take(i)), itemNames.Values.ElementAt(i - 1));

                    result.Add(path);
                }
            }

            // For each parent category create a separate outline: catalog/parent_category
            if (items.Count > 2)
            {
                var catalogId = items.First();

                result.AddRange(
                    items.Skip(1)
                        .Select(i => !getNameLatestItem ?
                                string.Join("/", catalogId, i) :
                                string.Join(ModuleConstants.OutlineDelimiter,
                                    string.Join("/", catalogId, i), itemNames[i])
                        ));
            }

            return result;
        }
    }
}
