using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extenstions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class ProductDocumentBuilder : CatalogDocumentBuilder, IIndexDocumentBuilder
    {
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productsSearchService;

        public ProductDocumentBuilder(ISettingsManager settingsManager, IItemService itemService, IProductSearchService productsSearchService)
            : base(settingsManager)
        {
            _itemService = itemService;
            _productsSearchService = productsSearchService;
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var result = new List<IndexDocument>();
            var products = await GetProducts(documentIds);
            foreach (var product in products)
            {
                var doc = CreateDocument(product);
                result.Add(doc);

                //Index product variants by separate chunked requests for performance reason
                if (product.MainProductId == null)
                {
                    var variationsSearchCriteria = GetVariationSearchCriteria(product);
                    var skipCount = 0;
                    int totalCount;
                    do
                    {
                        variationsSearchCriteria.Skip = skipCount;
                        var productVariations = await _productsSearchService.SearchNoCloneAsync(variationsSearchCriteria);
                        foreach (var variation in productVariations.Results)
                        {
                            result.Add(CreateDocument(variation, product));
                            IndexProductVariation(doc, variation);
                        }
                        totalCount = productVariations.TotalCount;
                        skipCount += variationsSearchCriteria.Take;
                    }
                    while (skipCount < totalCount);
                }
            }

            // Forcibly clear products from the cache to reduce memory consumption
            ItemCacheRegion.ExpireRegion();
            GC.Collect();

            return result;
        }

        protected virtual async Task<CatalogProduct[]> GetProducts(IList<string> productIds)
        {
#pragma warning disable CS0618 // Variations can be used here
            var products = await _itemService.GetNoCloneAsync(productIds.ToList(), (ItemResponseGroup.Full & ~ItemResponseGroup.Variations).ToString());
#pragma warning restore CS0618

            return products.ToArray();
        }

        protected virtual ProductSearchCriteria GetVariationSearchCriteria(CatalogProduct product)
        {
            var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();

            criteria.MainProductId = product.Id;
            criteria.ResponseGroup = (ItemResponseGroup.ItemInfo | ItemResponseGroup.Properties | ItemResponseGroup.Seo | ItemResponseGroup.Outlines | ItemResponseGroup.ItemAssets | ItemResponseGroup.ItemEditorialReviews).ToString();
            criteria.Take = 50;

            return criteria;
        }

        /// <summary>
        /// The mainProduct argument contains more information than variation.MainProduct
        /// </summary>
        protected virtual IndexDocument CreateDocument(CatalogProduct variation, CatalogProduct mainProduct)
        {
            return CreateDocument(variation);
        }

        protected virtual IndexDocument CreateDocument(CatalogProduct product)
        {
            var document = new IndexDocument(product.Id);

            document.AddFilterableValue("__type", product.GetType().Name, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("__sort", product.Name, IndexDocumentFieldValueType.String);

            var statusField = product.IsActive != true || product.MainProductId != null ? "hidden" : "visible";
            IndexIsProperty(document, statusField);
            IndexIsProperty(document, string.IsNullOrEmpty(product.MainProductId) ? "product" : "variation");
            IndexIsProperty(document, product.Code);

            document.AddFilterableValue("status", statusField, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("outerid", product.OuterId, IndexDocumentFieldValueType.String);
            document.AddFilterableAndSearchableValue("sku", product.Code);
            document.AddFilterableAndSearchableValue("code", product.Code);
            document.AddSuggestableValue("name", product.Name);
            document.AddFilterableValue("startdate", product.StartDate, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("enddate", product.EndDate ?? DateTime.MaxValue, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("createddate", product.CreatedDate, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("lastmodifieddate", product.ModifiedDate ?? DateTime.MaxValue, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("modifieddate", product.ModifiedDate ?? DateTime.MaxValue, IndexDocumentFieldValueType.DateTime);
            document.AddFilterableValue("priority", product.Priority, IndexDocumentFieldValueType.Integer);
            document.AddFilterableValue("vendor", product.Vendor ?? string.Empty, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("productType", product.ProductType ?? string.Empty, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("mainProductId", product.MainProductId ?? string.Empty, IndexDocumentFieldValueType.String);
            document.AddFilterableValue("gtin", product.Gtin ?? string.Empty, IndexDocumentFieldValueType.String);

            var productAvailability = GetProductAvailability(product);
            if (!string.IsNullOrEmpty(productAvailability))
            {
                document.AddFilterableValue("availability", productAvailability, IndexDocumentFieldValueType.String);
            }


            // Add priority in virtual categories to search index
            if (product.Links != null)
            {
                foreach (var link in product.Links)
                {
                    document.AddFilterableValue($"priority_{link.CatalogId}_{link.CategoryId}", link.Priority, IndexDocumentFieldValueType.Integer);
                }
            }

            // Add catalogs to search index
            var catalogs = product.Outlines
                .Select(o => o.Items.First().Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            document.AddFilterableValues("catalog", catalogs);

            // Add outlines to search index
            var outlineStrings = GetOutlineStrings(product.Outlines);
            document.AddFilterableValues("__outline", outlineStrings);

            document.AddFilterableValues("__outline_named", GetOutlineStrings(product.Outlines, getNameLatestItem: true));

            // Add the all physical and virtual paths
            document.AddFilterableValues("__path", product.Outlines.Select(x => string.Join("/", x.Items.Take(x.Items.Count - 1).Select(i => i.Id))).ToList());

            // Types of properties which values should be added to the searchable __content field
            var contentPropertyTypes = new[] { PropertyType.Product, PropertyType.Variation };

            // Index custom product properties
            IndexCustomProperties(document, product.Properties, contentPropertyTypes);

            // Index editorial reviews
            IndexDescriptions(document, product.Reviews);

            if (StoreObjectsInIndex)
            {
                // Index serialized product
                document.AddObjectFieldValue(product);
            }

            return document;
        }

        protected virtual void IndexProductVariation(IndexDocument document, CatalogProduct variation)
        {
            if (variation.ProductType == "Physical")
            {
                document.Add(new IndexDocumentField("type", "physical") { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
                IndexIsProperty(document, "physical");
            }

            if (variation.ProductType == "Digital")
            {
                document.Add(new IndexDocumentField("type", "digital") { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
                IndexIsProperty(document, "digital");
            }

            if (variation.ProductType == "BillOfMaterials")
            {
                document.Add(new IndexDocumentField("type", "billofmaterials") { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
                IndexIsProperty(document, "billofmaterials");
            }

            // add the variation code to content
            document.Add(new IndexDocumentField("__content", variation.Code) { IsRetrievable = true, IsSearchable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            // add the variationId to __variations
            document.Add(new IndexDocumentField("__variations", variation.Id) { IsRetrievable = true, IsSearchable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });

            IndexCustomProperties(document, variation.Properties, new[] { PropertyType.Variation });
            IndexDescriptions(document, variation.Reviews);
        }

        protected virtual void IndexDescriptions(IndexDocument document, IList<EditorialReview> reviews)
        {
            foreach (var review in reviews.Where(x => !string.IsNullOrEmpty(x?.Content)))
            {
                var descriptionField = $"description_{review.ReviewType.ToLowerInvariant()}_{review.LanguageCode.ToLowerInvariant()}";
                document.Add(new IndexDocumentField(descriptionField, review.Content) { IsRetrievable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
            }
        }

        protected virtual void IndexIsProperty(IndexDocument document, string value)
        {
            document.Add(new IndexDocumentField("is", value) { IsRetrievable = true, IsFilterable = true, IsCollection = true, ValueType = IndexDocumentFieldValueType.String, });
        }

        protected virtual string GetProductAvailability(CatalogProduct product)
        {
            if (!product.IsActive.GetValueOrDefault(true))
            {
                return "SoldOut";
            }

            if (!product.IsBuyable.GetValueOrDefault(true))
            {
                return "OutOfStock";
            }

            if (!product.TrackInventory.GetValueOrDefault(true))
            {
                return "InStock";
            }

            return null;
        }
    }
}
