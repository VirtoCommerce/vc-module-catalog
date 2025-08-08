using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Seo.Core.Models;
using static VirtoCommerce.SearchModule.Core.Extensions.IndexDocumentExtensions;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public abstract class CatalogDocumentBuilder
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IPropertySearchService _propertySearchService;

        protected CatalogDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService)
        {
            _settingsManager = settingsManager;
            _propertySearchService = propertySearchService;
        }

        protected virtual bool StoreObjectsInIndex => _settingsManager.GetValue<bool>(ModuleConstants.Settings.Search.UseFullObjectIndexStoring);

        protected virtual async Task AddCustomPropertiesAsync(IndexDocument schema, params PropertyType[] propertyTypes)
        {
            object booleanValue = default(bool);
            object dateTimeValue = default(DateTime);
            object integerValue = default(int);
            object doubleValue = default(double);
            object stringValue = SchemaStringValue;
            object geoPointValue = new GeoPoint();

            var searchCriteria = AbstractTypeFactory<PropertySearchCriteria>.TryCreateInstance();
            searchCriteria.PropertyTypes = propertyTypes.Select(x => x.ToString()).ToArray();

            await foreach (var searchResult in _propertySearchService.SearchBatches(searchCriteria))
            {
                foreach (var property in searchResult.Results)
                {
                    var propertyName = property.Name.ToLowerInvariant();
                    var isCollection = property.Multivalue || property.Type == PropertyType.Variation;

                    switch (property.ValueType)
                    {
                        case PropertyValueType.Boolean:
                            schema.Add(new IndexDocumentField(propertyName, booleanValue, IndexDocumentFieldValueType.Boolean) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.DateTime:
                            schema.Add(new IndexDocumentField(propertyName, dateTimeValue, IndexDocumentFieldValueType.DateTime) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.Integer:
                            schema.Add(new IndexDocumentField(propertyName, integerValue, IndexDocumentFieldValueType.Integer) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.Number:
                            schema.Add(new IndexDocumentField(propertyName, doubleValue, IndexDocumentFieldValueType.Double) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.ShortText:
                        case PropertyValueType.Color:
                            schema.Add(new IndexDocumentField(propertyName, stringValue, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.LongText:
                        case PropertyValueType.Html:
                            schema.Add(new IndexDocumentField(propertyName, stringValue, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsSearchable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.GeoPoint:
                            schema.Add(new IndexDocumentField(propertyName, geoPointValue, IndexDocumentFieldValueType.GeoPoint) { IsRetrievable = true, IsSearchable = true, IsCollection = true });
                            break;
                    }
                }
            }
        }

        protected virtual void AddObjectField<T>(IndexDocument schema, T value)
            where T : IHasProperties
        {
            var propertyValue = new PropertyValue { Value = SchemaStringValue, ValueType = PropertyValueType.ShortText };
            var property = new Property { Values = new List<PropertyValue> { propertyValue } };
            propertyValue.Property = property;
            value.Properties = new List<Property> { property };
            schema.AddObjectFieldValue(value);
        }

        protected virtual void IndexCustomProperties(IndexDocument document, ICollection<Property> properties, ICollection<PropertyType> contentPropertyTypes)
        {
            foreach (var property in properties.Where(x => x != null))
            {
                IndexCustomProperty(document, property, contentPropertyTypes);
            }
        }

        protected virtual void IndexCustomProperty(IndexDocument document, Property property, ICollection<PropertyType> contentPropertyTypes, bool addField = true)
        {
            var isCollection = property.Multivalue || property.Type == PropertyType.Variation;

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
                        case PropertyValueType.Html:
                            document.Add(new IndexDocumentField(propertyName, propValue.Value.ToString().ToLowerInvariant(), IndexDocumentFieldValueType.String) { IsRetrievable = true, IsSearchable = true, IsCollection = isCollection });
                            break;
                        case PropertyValueType.ShortText:
                        case PropertyValueType.Color:
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
                    switch (propValue.ValueType)
                    {
                        case PropertyValueType.LongText:
                        case PropertyValueType.ShortText:
                        case PropertyValueType.Color:
                            var stringValue = propValue.Value.ToString();
                            document.AddContentString(stringValue,
                                property.Multilanguage ? propValue.LanguageCode : string.Empty);
                            break;
                    }
                }
            }
        }

        protected virtual void IndexSeoInformation(IndexDocument document, IList<SeoInfo> seoInfos)
        {
            foreach (var seoInfo in seoInfos)
            {
                document.AddContentString(seoInfo.MetaKeywords, seoInfo.LanguageCode);
                document.AddContentString(seoInfo.MetaDescription, seoInfo.LanguageCode);
                document.AddContentString(seoInfo.PageTitle, seoInfo.LanguageCode);
            }
        }

        protected virtual string[] GetOutlineStrings(IEnumerable<Outline> outlines, bool withName = false)
        {
            return outlines
                .SelectMany(x => ExpandOutline(x, withName))
                .Distinct()
                .ToArray();
        }

        protected virtual IEnumerable<string> ExpandOutline(Outline outline, bool withName)
        {
            // Outline structure: catalog/category1/.../categoryN/current-item

            var itemIds = outline.Items
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
            if (itemIds.Count > 0)
            {
                for (var i = itemIds.Count; i > 0; i--)
                {
                    var name = withName ? itemNames.Values.ElementAt(i - 1) : null;
                    var outlineString = OutlineString.Build(itemIds.Take(i), name);
                    result.Add(outlineString);
                }
            }

            // For each parent category create a separate outline: catalog/parent_category
            if (itemIds.Count > 2)
            {
                var catalogId = itemIds.First();

                result.AddRange(
                    itemIds.Skip(1)
                        .Select(id =>
                        {
                            var name = withName ? itemNames[id] : null;
                            var outlineString = OutlineString.Build([catalogId, id], name);

                            return outlineString;
                        }));
            }

            return result;
        }
    }
}
