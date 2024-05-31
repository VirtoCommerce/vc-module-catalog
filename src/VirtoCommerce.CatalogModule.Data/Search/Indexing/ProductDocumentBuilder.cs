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
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class ProductDocumentBuilder : CatalogDocumentBuilder, IIndexSchemaBuilder, IIndexDocumentBuilder
    {
        private readonly IProductService _productService;
        private readonly IProductSearchService _productsSearchService;

        public ProductDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService, IProductService productService, IProductSearchService productsSearchService)
            : base(settingsManager, propertySearchService)
        {
            _productService = productService;
            _productsSearchService = productsSearchService;
        }

        [Obsolete($"Use the overload that accepts {nameof(IProductService)}")]
        public ProductDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService, IItemService itemService, IProductSearchService productsSearchService)
            : this(settingsManager, propertySearchService, (IProductService)itemService, productsSearchService)
        {
        }

        [Obsolete($"This constructor is intended to be used by a DI container only")]
        public ProductDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService, IProductService productService, /* ReSharper disable once UnusedParameter.Local */ IItemService itemService, IProductSearchService productsSearchService)
            : this(settingsManager, propertySearchService, productService, productsSearchService)
        {
        }

        public virtual async Task BuildSchemaAsync(IndexDocument schema)
        {
            schema.AddFilterableString("__type");
            schema.AddFilterableString("__sort");
            schema.AddFilterableString("status");
            schema.AddFilterableString("outerid");
            schema.AddFilterableStringAndContentString("sku");
            schema.AddFilterableStringAndContentString("code");
            schema.AddSuggestableStringAndContentString("name");

            schema.AddFilterableDateTime("startdate");
            schema.AddFilterableDateTime("enddate");
            schema.AddFilterableDateTime("createddate");
            schema.AddFilterableDateTime("lastmodifieddate");
            schema.AddFilterableDateTime("modifieddate");

            schema.AddFilterableInteger("priority");

            schema.AddFilterableString("vendor");
            schema.AddFilterableString("productType");
            schema.AddFilterableString("mainProductId");
            schema.AddFilterableString("productFamilyId");
            schema.AddFilterableString("gtin");
            schema.AddFilterableString("availability");

            schema.AddFilterableCollection("is");
            schema.AddFilterableCollection("catalog");
            schema.AddFilterableCollection("__outline");
            schema.AddFilterableCollection("__outline_named");
            schema.AddFilterableCollection("__path");

            schema.AddFilterableCollection("type");
            schema.AddSearchableCollection("__variations");

            AddObjectField(schema, AbstractTypeFactory<CatalogProduct>.TryCreateInstance());

            await AddCustomPropertiesAsync(schema, PropertyType.Product, PropertyType.Variation);
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

                            if (variation.IsActive.HasValue && variation.IsActive.Value)
                            {
                                IndexProductVariation(doc, variation);
                            }
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
            var products = await _productService.GetNoCloneAsync(productIds.ToList(), (ItemResponseGroup.Full & ~ItemResponseGroup.Variations).ToString());
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

            document.AddFilterableString("__type", product.GetType().Name);
            document.AddFilterableString("__sort", product.Name);

            var statusField = GetProductStatus(product);
            IndexIsProperty(document, statusField);
            IndexIsProperty(document, string.IsNullOrEmpty(product.MainProductId) ? "product" : "variation");
            IndexIsProperty(document, product.Code);

            document.AddFilterableString("status", statusField);
            document.AddFilterableString("outerid", product.OuterId);
            document.AddFilterableStringAndContentString("sku", product.Code);
            document.AddFilterableStringAndContentString("code", product.Code);
            document.AddSuggestableStringAndContentString("name", product.Name);
            document.AddFilterableDateTime("startdate", product.StartDate);
            document.AddFilterableDateTime("enddate", product.EndDate ?? DateTime.MaxValue);
            document.AddFilterableDateTime("createddate", product.CreatedDate);
            document.AddFilterableDateTime("lastmodifieddate", product.ModifiedDate ?? DateTime.MaxValue);
            document.AddFilterableDateTime("modifieddate", product.ModifiedDate ?? DateTime.MaxValue);
            document.AddFilterableInteger("priority", product.Priority);
            document.AddFilterableString("vendor", product.Vendor ?? string.Empty);
            document.AddFilterableString("productType", product.ProductType ?? string.Empty);
            document.AddFilterableString("mainProductId", product.MainProductId ?? string.Empty);
            document.AddFilterableString("productFamilyId", string.IsNullOrEmpty(product.MainProductId) ? product.Id : product.MainProductId);
            document.AddFilterableString("gtin", product.Gtin ?? string.Empty);

            var productAvailability = GetProductAvailability(product);
            if (!string.IsNullOrEmpty(productAvailability))
            {
                document.AddFilterableString("availability", productAvailability);
            }

            // Add priority in virtual categories to search index
            if (product.Links != null)
            {
                foreach (var link in product.Links)
                {
                    document.AddFilterableInteger($"priority_{link.CatalogId}_{link.CategoryId}", link.Priority);
                }
            }

            // Add catalogs to search index
            var catalogs = product.Outlines
                .Select(o => o.Items.First().Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            document.AddFilterableCollection("catalog", catalogs);

            // Add outlines to search index
            var outlineStrings = GetOutlineStrings(product.Outlines);
            document.AddFilterableCollection("__outline", outlineStrings);

            document.AddFilterableCollection("__outline_named", GetOutlineStrings(product.Outlines, getNameLatestItem: true));

            // Add all physical and virtual paths
            document.AddFilterableCollection("__path", product.Outlines.Select(x => string.Join("/", x.Items.Take(x.Items.Count - 1).Select(i => i.Id))).ToList());

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
            switch (variation.ProductType)
            {
                case "Physical":
                case "Digital":
                case "BillOfMaterials":
                    IndexTypeProperty(document, variation.ProductType.ToLowerInvariant());
                    break;
            }

            // add the variation code to content
            document.AddContentString(variation.Code);
            // add the variationId to __variations
            document.AddSearchableCollection("__variations", variation.Id);

            IndexCustomProperties(document, variation.Properties, new[] { PropertyType.Variation });
            IndexDescriptions(document, variation.Reviews);
        }

        protected virtual void IndexTypeProperty(IndexDocument document, string value)
        {
            document.AddFilterableCollection("type", value);
            IndexIsProperty(document, value);
        }

        protected virtual void IndexDescriptions(IndexDocument document, IList<EditorialReview> reviews)
        {
            foreach (var review in reviews.Where(x => !string.IsNullOrEmpty(x?.Content)))
            {
                var descriptionField = $"description_{review.ReviewType.ToLowerInvariant()}_{review.LanguageCode.ToLowerInvariant()}";
                document.Add(new IndexDocumentField(descriptionField, review.Content, IndexDocumentFieldValueType.String) { IsRetrievable = true, IsCollection = true });
            }
        }

        protected virtual void IndexIsProperty(IndexDocument document, string value)
        {
            document.AddFilterableCollection("is", value);
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

        protected virtual string GetProductStatus(CatalogProduct product)
        {
            return IsVisible(product) ? "visible" : "hidden";
        }

        /// <summary>
        /// Product must inherit "hidden" flag from parent categories
        /// </summary>
        private static bool IsVisible(CatalogProduct product)
        {
            return
                product.MainProductId == null &&
                product.IsActive == true &&
                product.ParentCategoryIsActive;
        }
    }
}
