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
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productsSearchService;
        private readonly IMeasureService _measureService;
        private readonly PropertyType[] _contentPropertyTypes = [PropertyType.Product, PropertyType.Variation];

        public ProductDocumentBuilder(
            ISettingsManager settingsManager,
            IPropertySearchService propertySearchService,
            IItemService itemService,
            IProductSearchService productsSearchService,
            IMeasureService measureService)
            : base(settingsManager, propertySearchService)
        {
            _itemService = itemService;
            _productsSearchService = productsSearchService;
            _measureService = measureService;
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
            schema.AddFilterableString("manufacturerPartNumber");
            schema.AddFilterableString("availability");
            schema.AddFilterableString("weightUnit");
            schema.AddFilterableDecimal("weight");
            schema.AddFilterableString("measureUnit");
            schema.AddFilterableDecimal("height");
            schema.AddFilterableDecimal("width");
            schema.AddFilterableDecimal("length");

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
                var doc = await CreateDocumentAsync(product);
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
                            result.Add(await CreateDocumentAsync(variation, product));

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

        [Obsolete("Use CreateDocumentAsync(CatalogProduct variation, CatalogProduct mainProduct)", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
        protected virtual IndexDocument CreateDocument(CatalogProduct variation, CatalogProduct mainProduct)
        {
            return CreateDocument(variation);
        }

        /// <summary>
        /// The mainProduct argument contains more information than variation.MainProduct
        /// </summary>
        protected async virtual Task<IndexDocument> CreateDocumentAsync(CatalogProduct variation, CatalogProduct mainProduct)
        {
#pragma warning disable VC0010 // Type or member is obsolete
            var document = CreateDocument(variation, mainProduct);
#pragma warning restore VC0010 // Type or member is obsolete

            await IndexMeasurePropertiesAsync(document, variation.Properties, _contentPropertyTypes);

            return document;

        }

        [Obsolete("Use CreateDocumentAsync(CatalogProduct product)", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
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
            IndexLocalizedName(document, product.LocalizedName);
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
            document.AddFilterableString("manufacturerPartNumber", product.ManufacturerPartNumber ?? string.Empty);
            document.AddFilterableString("weightUnit", product.WeightUnit ?? string.Empty);
            document.AddFilterableDecimal("weight", product.Weight);
            document.AddFilterableString("measureUnit", product.MeasureUnit ?? string.Empty);
            document.AddFilterableDecimal("height", product.Height);
            document.AddFilterableDecimal("width", product.Width);
            document.AddFilterableDecimal("length", product.Length);

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

            // Index custom product properties
            IndexCustomProperties(document, product.Properties, _contentPropertyTypes);

            // Index product descriptions
            IndexDescriptions(document, product.Reviews);

            IndexSeoInformation(document, product.SeoInfos);

            if (StoreObjectsInIndex)
            {
                // Index serialized product
                document.AddObjectFieldValue(product);
            }

            return document;
        }

        protected virtual async Task<IndexDocument> CreateDocumentAsync(CatalogProduct product)
        {
#pragma warning disable VC0010 // Type or member is obsolete
            var document = CreateDocument(product);
#pragma warning restore VC0010 // Type or member is obsolete

            await IndexMeasurePropertiesAsync(document, product.Properties, _contentPropertyTypes);

            return document;
        }

        protected virtual async Task IndexMeasurePropertiesAsync(IndexDocument document, IList<Property> properties, PropertyType[] contentPropertyTypes)
        {
            var measureProperties = properties.Where(p => p.ValueType == PropertyValueType.Measure && !string.IsNullOrEmpty(p.MeasureId)).ToList();

            foreach (var measureProperty in measureProperties)
            {
                var propValue = measureProperty.Values.FirstOrDefault();

                if (propValue != null)
                {
                    var measure = await _measureService.GetByIdAsync(measureProperty.MeasureId);
                    var valueUnitOfMeasure = measure?.Units.FirstOrDefault(u => u.Id == propValue.UnitOfMeasureId);

                    if (measure != null && valueUnitOfMeasure != null)
                    {
                        var propertyName = measureProperty.Name;
                        var defaultUnitValue = (decimal)propValue.Value * valueUnitOfMeasure.ConversionFactor;
                        document.Add(new IndexDocumentField(propertyName, defaultUnitValue, IndexDocumentFieldValueType.Double) { IsRetrievable = true, IsFilterable = true, IsCollection = false });

                        foreach (var unit in measure.Units.Where(u => !u.IsDefault))
                        {
                            var unitValue = defaultUnitValue / unit.ConversionFactor;
                            document.Add(new IndexDocumentField($"{propertyName}_{unit.Code}", unitValue, IndexDocumentFieldValueType.Double) { IsRetrievable = true, IsFilterable = true, IsCollection = false });
                        }
                    }
                }
            }
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

            IndexCustomProperties(document, variation.Properties, [PropertyType.Variation]);
            IndexDescriptions(document, variation.Reviews);
            IndexSeoInformation(document, variation.SeoInfos);
        }

        protected virtual void IndexLocalizedName(IndexDocument document, LocalizedString localizedString)
        {
            if (localizedString != null)
            {
                foreach (var languageCode in localizedString.Values.Keys)
                {
                    document.AddContentString(localizedString.GetValue(languageCode), languageCode);
                }
            }
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
                document.AddContentString(review.Content, review.LanguageCode);

                var descriptionField = $"description_{review.ReviewType?.ToLowerInvariant() ?? "null"}_{review.LanguageCode?.ToLowerInvariant() ?? "null"}";
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
                product.IsActive == true &&
                product.ParentCategoryIsActive;
        }
    }
}
