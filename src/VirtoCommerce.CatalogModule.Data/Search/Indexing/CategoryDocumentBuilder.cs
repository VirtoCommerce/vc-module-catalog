using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class CategoryDocumentBuilder : CatalogDocumentBuilder, IIndexSchemaBuilder, IIndexDocumentBuilder
    {
        private readonly ICategoryService _categoryService;

        public CategoryDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService, ICategoryService categoryService)
            : base(settingsManager, propertySearchService)
        {
            _categoryService = categoryService;
        }

        public virtual async Task BuildSchemaAsync(IndexDocument schema)
        {
            schema.AddFilterableString("__key");
            schema.AddFilterableString("__type");
            schema.AddFilterableString("__sort");
            schema.AddFilterableString("status");
            schema.AddFilterableString("outerid");

            schema.AddFilterableStringAndContentString("code");
            schema.AddFilterableStringAndContentString("name");

            schema.AddFilterableDateTime("createddate");
            schema.AddFilterableDateTime("lastmodifieddate");
            schema.AddFilterableDateTime("modifieddate");

            schema.AddFilterableInteger("priority");

            schema.AddFilterableCollection("is");
            schema.AddFilterableCollection("catalog");
            schema.AddFilterableCollection("__outline");
            schema.AddFilterableCollection("__outline_named");

            schema.AddFilterableString("parent");

            AddObjectField(schema, AbstractTypeFactory<CatalogProduct>.TryCreateInstance());

            await AddCustomPropertiesAsync(schema, PropertyType.Category);
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var categories = await GetCategories(documentIds);

            IList<IndexDocument> result = categories
                .Select(CreateDocument)
                .Where(doc => doc != null)
                .ToArray();

            return result;
        }


        protected virtual async Task<Category[]> GetCategories(IList<string> categoryIds)
        {
            var responseGroup = CategoryResponseGroup.WithProperties | CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithImages | CategoryResponseGroup.WithSeo | CategoryResponseGroup.WithLinks | CategoryResponseGroup.WithDescriptions;
            var categories = await _categoryService.GetNoCloneAsync(categoryIds.ToList(), responseGroup.ToString());

            return categories.ToArray();
        }

        protected virtual IndexDocument CreateDocument(Category category)
        {
            var document = new IndexDocument(category.Id);

            document.AddFilterableString("__key", category.Id.ToLowerInvariant());
            document.AddFilterableString("__type", category.GetType().Name);
            document.AddFilterableString("__sort", category.Name);

            var statusField = IsActiveInTotal(category) ? "visible" : "hidden";
            IndexIsProperty(document, statusField);
            IndexIsProperty(document, "category");
            IndexIsProperty(document, category.Code);

            document.AddFilterableString("status", statusField);
            document.AddFilterableString("outerid", category.OuterId);
            document.AddFilterableStringAndContentString("code", category.Code);
            document.AddFilterableStringAndContentString("name", category.Name);
            document.AddFilterableDateTime("createddate", category.CreatedDate);
            document.AddFilterableDateTime("lastmodifieddate", category.ModifiedDate ?? DateTime.MaxValue);
            document.AddFilterableDateTime("modifieddate", category.ModifiedDate ?? DateTime.MaxValue);
            document.AddFilterableInteger("priority", category.Priority);

            // Add priority in virtual categories to search index
            if (category.Links != null)
            {
                foreach (var link in category.Links)
                {
                    document.AddFilterableInteger($"priority_{link.CatalogId}_{link.CategoryId}", link.Priority);
                }
            }

            // Add catalogs to search index
            var catalogs = category.Outlines
                .Select(o => o.Items.First().Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var catalogId in catalogs)
            {
                document.AddFilterableCollection("catalog", catalogId.ToLowerInvariant());
            }

            // Add outlines to search index
            var outlineStrings = GetOutlineStrings(category.Outlines);
            foreach (var outline in outlineStrings)
            {
                document.AddFilterableCollection("__outline", outline.ToLowerInvariant());
            }

            foreach (var outlineItem in GetOutlineStrings(category.Outlines, getNameLatestItem: true))
            {
                document.AddFilterableCollection("__outline_named", outlineItem);
            }

            IndexCustomProperties(document, category.Properties, new[] { PropertyType.Category });

            // Index product descriptions
            IndexDescriptions(document, category.Descriptions);

            // Index seo information
            IndexSeoInformation(document, category.SeoInfos);

            if (StoreObjectsInIndex)
            {
                // Index serialized category
                document.AddObjectFieldValue(category);
            }

            document.AddFilterableString("parent", category.ParentId ?? category.CatalogId);

            return document;
        }

        protected virtual void IndexDescriptions(IndexDocument document, IList<CategoryDescription> descriptions)
        {
            foreach (var description in descriptions.Where(x => !string.IsNullOrEmpty(x?.Content)))
            {
                document.AddContentString(description.Content);

                if (!string.IsNullOrEmpty(description.LanguageCode))
                {
                    document.AddContentString(description.Content, description.LanguageCode);
                }

                var descriptionField = $"description_{description.DescriptionType?.ToLowerInvariant() ?? "null"}_{description.LanguageCode?.ToLowerInvariant() ?? "null"}";
                document.Add(new IndexDocumentField(descriptionField, description.Content, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsCollection = true });
            }
        }

        /// <summary>
        /// Child categories must inherit "hidden" flag from parent categories
        /// </summary>
        protected virtual bool IsActiveInTotal(Category category)
        {
            return category.IsActive == true && category.ParentIsActive;
        }

        protected virtual void IndexIsProperty(IndexDocument document, string value)
        {
            document.AddFilterableCollection("is", value);
        }
    }
}
