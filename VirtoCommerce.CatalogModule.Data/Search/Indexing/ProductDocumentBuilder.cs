using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class ProductDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly ISettingsManager _settingsManager;

        public ProductDocumentBuilder(IItemService itemService, IBlobUrlResolver blobUrlResolver, ISettingsManager settingsManager)
        {
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
            _settingsManager = settingsManager;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var products = GetProducts(documentIds);

            IList<IndexDocument> result = products.Select(CreateDocument).ToArray();
            return Task.FromResult(result);
        }


        protected virtual IList<CatalogProduct> GetProducts(IList<string> productIds)
        {
            return _itemService.GetByIds(productIds.ToArray(), ItemResponseGroup.ItemProperties | ItemResponseGroup.Variations | ItemResponseGroup.Outlines | ItemResponseGroup.Seo);
        }

        protected virtual IndexDocument CreateDocument(CatalogProduct product)
        {
            var document = new IndexDocument(product.Id);

            document.Add(new IndexDocumentField("__type", product.GetType().Name) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("__sort", product.Name) { IsRetrievable = true, IsFilterable = true });

            var statusField = product.IsActive != true || product.MainProductId != null ? "hidden" : "visible";
            IndexIsProperty(document, statusField);
            IndexIsProperty(document, "product");
            IndexIsProperty(document, product.Code);

            document.Add(new IndexDocumentField("status", statusField) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("code", product.Code) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("name", product.Name) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("startdate", product.StartDate) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("enddate", product.EndDate ?? DateTime.MaxValue) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("createddate", product.CreatedDate) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("lastmodifieddate", product.ModifiedDate ?? DateTime.MaxValue) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("priority", product.Priority) { IsRetrievable = true, IsFilterable = true });
            document.Add(new IndexDocumentField("vendor", product.Vendor ?? "") { IsRetrievable = true, IsFilterable = true });

            // Add priority in virtual categories to search index
            foreach (var link in product.Links)
            {
                document.Add(new IndexDocumentField(string.Format(CultureInfo.InvariantCulture, "priority_{0}_{1}", link.CatalogId, link.CategoryId), link.Priority) { IsRetrievable = true, IsFilterable = true });
            }

            // Add catalogs to search index
            var catalogs = product.Outlines
                .Select(o => o.Items.First().Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var catalogId in catalogs)
            {
                document.Add(new IndexDocumentField("catalog", catalogId.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }

            // Add outlines to search index
            var outlineStrings = GetOutlineStrings(product.Outlines);
            foreach (var outline in outlineStrings)
            {
                document.Add(new IndexDocumentField("__outline", outline.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }

            // Index custom properties
            IndexItemCustomProperties(document, product);

            if (product.Variations != null)
            {
                if (product.Variations.Any(c => c.ProductType == "Physical"))
                {
                    document.Add(new IndexDocumentField("type", "physical") { IsRetrievable = true, IsFilterable = true, IsCollection = true });
                    IndexIsProperty(document, "physical");
                }

                if (product.Variations.Any(c => c.ProductType == "Digital"))
                {
                    document.Add(new IndexDocumentField("type", "digital") { IsRetrievable = true, IsFilterable = true, IsCollection = true });
                    IndexIsProperty(document, "digital");
                }

                foreach (var variation in product.Variations)
                {
                    IndexItemCustomProperties(document, variation);
                }
            }

            // add to content
            document.Add(new IndexDocumentField("__content", product.Name) { IsRetrievable = true, IsSearchable = true, IsCollection = true });
            document.Add(new IndexDocumentField("__content", product.Code) { IsRetrievable = true, IsSearchable = true, IsCollection = true });

            if (_settingsManager.GetValue("VirtoCommerce.SearchApi.UseFullObjectIndexStoring", true))
            {
                // Index serialized product
                var itemDto = product.ToWebModel(_blobUrlResolver);
                document.AddObjectFieldValue(itemDto);
            }

            return document;
        }
        protected virtual void IndexIsProperty(IndexDocument document, string value)
        {
            document.Add(new IndexDocumentField("is", value) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
        }

        protected virtual void IndexItemCustomProperties(IndexDocument document, CatalogProduct item)
        {
            var properties = item.Properties;

            foreach (var propValue in item.PropertyValues.Where(x => x.Value != null))
            {
                var propertyName = (propValue.PropertyName ?? "").ToLowerInvariant();
                var property = properties.FirstOrDefault(x => string.Equals(x.Name, propValue.PropertyName, StringComparison.InvariantCultureIgnoreCase) && x.ValueType == propValue.ValueType);
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
                    case PropertyValueType.ShortText: // do not tokenize small values as they will be used for lookups and filters
                        document.Add(new IndexDocumentField(propertyName, propValue.Value.ToString()) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                        break;
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

        protected virtual string[] GetOutlineStrings(IEnumerable<Outline> outlines)
        {
            return outlines
                .SelectMany(ExpandOutline)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        protected virtual IEnumerable<string> ExpandOutline(Outline outline)
        {
            // Outline structure: catalog/category1/.../categoryN/product

            var items = outline.Items
                .Take(outline.Items.Count - 1) // Exclude last item, which is product ID
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
