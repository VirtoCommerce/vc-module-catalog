using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class CategoryDocumentBuilder : CatalogDocumentBuilder, IIndexDocumentBuilder
    {
        private readonly ICategoryService _categoryService;

        public CategoryDocumentBuilder(ISettingsManager settingsManager, ICategoryService categoryService)
            : base(settingsManager)
        {
            _categoryService = categoryService;
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


        protected virtual Task<Category[]> GetCategories(IList<string> categoryIds)
        {
            return _categoryService.GetByIdsAsync(categoryIds.ToArray(), (CategoryResponseGroup.WithProperties | CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithImages | CategoryResponseGroup.WithSeo | CategoryResponseGroup.WithLinks).ToString());
        }

        protected virtual IndexDocument CreateDocument(Category category)
        {
            var document = new IndexDocument(category.Id);

            document.Add(new IndexDocumentField("__key", category.Id.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("__type", category.GetType().Name) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("__sort", category.Name) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });

            var statusField = IsActiveInTotal(category) ? "visible" : "hidden";
            IndexIsProperty(document, statusField);
            IndexIsProperty(document, "category");
            IndexIsProperty(document, category.Code);

            document.Add(new IndexDocumentField("status", statusField) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("code", category.Code) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("name", category.Name) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("createddate", category.CreatedDate) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.DateTime, });
            document.Add(new IndexDocumentField("lastmodifieddate", category.ModifiedDate ?? DateTime.MaxValue) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.DateTime, });
            document.Add(new IndexDocumentField("modifieddate", category.ModifiedDate ?? DateTime.MaxValue) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.DateTime, });
            document.Add(new IndexDocumentField("priority", category.Priority) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.Integer, });

            // Add priority in virtual categories to search index
            if (category.Links != null)
            {
                foreach (var link in category.Links)
                {
                    document.Add(new IndexDocumentField($"priority_{link.CatalogId}_{link.CategoryId}", link.Priority) { IsRetrievable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.Integer, });
                }
            }

            // Add catalogs to search index
            var catalogs = category.Outlines
                .Select(o => o.Items.First().Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var catalogId in catalogs)
            {
                document.Add(new IndexDocumentField("catalog", catalogId.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            }

            // Add outlines to search index
            var outlineStrings = GetOutlineStrings(category.Outlines);
            foreach (var outline in outlineStrings)
            {
                document.Add(new IndexDocumentField("__outline", outline.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            }

            foreach (var outlineItem in GetOutlineStrings(category.Outlines, getNameLatestItem: true))
            {
                document.Add(new IndexDocumentField($"__outline_named", outlineItem) { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            }

            IndexCustomProperties(document, category.Properties, new[] { PropertyType.Category });

            // add to content
            document.Add(new IndexDocumentField("__content", category.Name) { IsRetrievable = true, IsSearchable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            document.Add(new IndexDocumentField("__content", category.Code) { IsRetrievable = true, IsSearchable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });

            if (StoreObjectsInIndex)
            {
                // Index serialized category
                document.AddObjectFieldValue(category);
            }

            document.Add(new IndexDocumentField("parent", category.ParentId ?? category.CatalogId) { IsRetrievable = true, IsSearchable = true, IsFilterable = true, ValueType = IndexDocumentFieldValueType.String, });

            return document;
        }

        /// <summary>
        /// Child categories must inherit "hidden" flag from parent categories
        /// </summary>
        protected virtual bool IsActiveInTotal(Category category)
        {
            bool result;
            if (category.IsActive.HasValue && !category.IsActive.Value)
            {
                result = false;
            }
            else
            {
                result = category.Parent == null
                    ? category.IsActive == true
                    : IsActiveInTotal(category.Parent);
            }

            return result;
        }

        protected virtual void IndexIsProperty(IndexDocument document, string value)
        {
            document.Add(new IndexDocumentField("is", value) { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
        }
    }
}
