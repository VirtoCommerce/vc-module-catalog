using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Serialization;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class CatalogProduct : AuditableEntity, IHasLinks, ISeoSupport, IHasOutlines, IHasDimension, IHasAssociations, IHasProperties, IHasImages, IHasAssets, IInheritable, IHasTaxType, IHasName, IHasOuterId, IExportable, ICopyable, IHasCategoryId, IHasExcludedProperties
    {
        /// <summary>
        /// The type of product. Can be "Physical", "Digital", etc.
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// The Stock Keeping Unit (SKU) code for the product.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A manufacturer part number (MPN) is a unique alphanumeric code assigned by a manufacturer to identify a specific product or component. It is used primarily for part tracking in inventory management, supply chain operations, and ordering purposes.
        /// </summary>
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// The Global Trade Item Number (GTIN) for the product. This can include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).
        /// </summary>
        public string Gtin { get; set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the catalog to which this product belongs.
        /// </summary>
        public string CatalogId { get; set; }

        /// <summary>
        /// The catalog to which this product belongs.
        /// </summary>
        [JsonIgnore]
        public Catalog Catalog { get; set; }

        /// <summary>
        /// The ID of the category to which this product belongs.
        /// </summary>
        public string CategoryId { get; set; }

        [JsonIgnore]
        public Category Category { get; set; }
        /// <summary>
        /// Product outline in physical catalog (all parent categories ids concatenated. E.g. (1/21/344))
        /// </summary>
        public string Outline => Category?.Outline;
        /// <summary>
        /// Product path in physical catalog (all parent categories names concatenated. E.g. (parent1/parent2))
        /// </summary>
        public string Path => Category?.Path;

        public string TitularItemId => MainProductId;
        /// <summary>
        /// The ID of the main product associated with this product variation.
        /// </summary>
        public string MainProductId { get; set; }

        /// <summary>
        /// The main product associated with this product variation.
        /// </summary>
        [JsonIgnore]
        public CatalogProduct MainProduct { get; set; }

        /// <summary>
        /// Specifies whether the product is currently visible on the store for customers to view and purchase.
        /// If set to false, the product is currently sold out.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        ///  Specifies whether the product is currently visible on the store for customers to view and purchase.
        ///  If set to false, the product is currently ouf of stock.
        /// </summary>
        public bool? IsBuyable { get; set; }

        /// <summary>
        /// Indicates whether the inventory service is tracking the availability of this product.
        /// If set to false, the product is considered in stock without any inventory limitations.
        /// </summary>
        public bool? TrackInventory { get; set; }

        /// <summary>
        /// The date and time when the product was last indexed for search.
        /// </summary>
        public DateTime? IndexingDate { get; set; }

        /// <summary>
        /// The maximum quantity of the product that can be purchased in a single order. A value of 0 indicates that there are no limitations on the maximum quantity.
        /// </summary>
        public int? MaxQuantity { get; set; }

        /// <summary>
        /// The minimum quantity of the product that must be purchased in a single order. A value of 0 indicates that there are no limitations on the minimum quantity.
        /// </summary>
        public int? MinQuantity { get; set; }

        /// <summary>
        /// Defines the number of items in a package. Quantity step for your product's. Default value is 1.
        /// </summary>
        public int PackSize { get; set; } = 1;

        /// <summary>
        /// First listed date and time. If you do not specify an end date, the product will be active until you deactivate it.If you do not specify an end date, the product will be active until you deactivate it.If you do not specify a start date, the product will become active immediately once you save it.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Listing expires on the specific date and time. If you do not specify an end date, the product will be active until you deactivate it.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The type of package for this product, which determines the product's specific dimensions.
        /// </summary>
        public string PackageType { get; set; }

        #region IHaveDimension Members
        /// <summary>
        /// The unit of measurement for the product's weight.
        /// </summary>
        public string WeightUnit { get; set; }

        /// <summary>
        /// The weight of the product, in the unit specified by the WeightUnit property.
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// The unit of measurement for the product's height, length, and width.
        /// </summary>
        public string MeasureUnit { get; set; }

        /// <summary>
        /// The height of the product, in the unit specified by the MeasureUnit property.
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// The length of the product, in the unit specified by the MeasureUnit property.
        /// </summary>
        public decimal? Length { get; set; }

        /// <summary>
        /// The width of the product, in the unit specified by the MeasureUnit property.
        /// </summary>
        public decimal? Width { get; set; }
        #endregion

        public bool? EnableReview { get; set; }

        /// <summary>
        /// The maximum number of times the product can be downloaded. A value of 0 indicates no limit.
        /// </summary>
        public int? MaxNumberOfDownload { get; set; }

        /// <summary>
        /// The date and time when the download link or access to the product will expire.
        /// </summary>
        public DateTime? DownloadExpiration { get; set; }

        /// <summary>
        /// The type of product download. Valid values include: "Standard Product", "Software", and "Music".
        /// </summary>
        public string DownloadType { get; set; }

        /// <summary>
        /// Indicates whether the product requires the user to agree to any terms or conditions before downloading.
        /// </summary>
        public bool? HasUserAgreement { get; set; }

        /// <summary>
        /// Specifies the type of shipping option available for the product.
        /// </summary>
        public string ShippingType { get; set; }

        /// <summary>
        /// Specifies the type of tax applied to the product.
        /// </summary>
        public string TaxType { get; set; }

        /// <summary>
        /// ID of the vendor associated with the product.
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Indicates the position of the product in the catalog for ordering purposes.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// An external identifier for the product that can be used for integration with external systems.
        /// </summary>
        public string OuterId { get; set; }


        #region IHasProperties members
        public IList<Property> Properties { get; set; }
        #endregion

        #region IHasExcludedProperties members
        public IList<ExcludedProperty> ExcludedProperties { get; set; }
        #endregion

        [JsonIgnoreSerialization]
        [Obsolete("it's for importing data from v.2, need to use values in Properties")]
        public ICollection<PropertyValue> PropertyValues { get; set; }

        #region IHasImages members
        /// <summary>
        /// Gets the default image for the product.
        /// </summary>
        /// <value>
        /// The image source URL.
        /// </value>
        public string ImgSrc
        {
            get
            {
                string result = null;
                if (Images != null && Images.Any())
                {
                    result = Images.MinBy(x => x.SortOrder)?.Url;
                }
                return result;
            }
        }

        public IList<Image> Images { get; set; }
        #endregion

        #region IHasAssets members
        public IList<Asset> Assets { get; set; }
        #endregion

        #region ILinkSupport members
        public IList<CategoryLink> Links { get; set; }
        #endregion

        public IList<Variation> Variations { get; set; }
        /// <summary>
        /// Each descendant type should override this property to use other object type for seo records
        /// </summary>
        public virtual string SeoObjectType => "CatalogProduct";

        public IList<SeoInfo> SeoInfos { get; set; }
        public IList<EditorialReview> Reviews { get; set; }

        #region IHasAssociations members
        public IList<ProductAssociation> Associations { get; set; }
        #endregion

        public IList<ProductAssociation> ReferencedAssociations { get; set; }

        #region IHasOutlines members
        public IList<Outline> Outlines { get; set; }
        #endregion

        #region IInheritable members
        /// <summary>
        /// System flag used to mark that object was inherited from other
        /// </summary>
        public virtual bool IsInherited => false;

        public virtual void TryInheritFrom(IEntity parent)
        {
            this.InheritExcludedProperties(parent as IHasExcludedProperties);

            InheritProperties(parent);

            InheritTaxes(parent);

            if (!Variations.IsNullOrEmpty())
            {
                foreach (var variation in Variations)
                {
                    variation.TryInheritFrom(this);
                }
            }

            InheritFromCatalogProduct(parent);
        }

        protected virtual void InheritFromCatalogProduct(IEntity parent)
        {
            if (parent is CatalogProduct parentProduct)
            {
                InheritImages(parentProduct);

                InheritAssets(parentProduct);

                InheritReviews(parentProduct);

                InheritPropertyValues(parentProduct);

                // TODO: prevent saving the inherited simple values
                Width = parentProduct.Width ?? Width;
                Height = parentProduct.Height ?? Height;
                Length = parentProduct.Length ?? Length;
                MeasureUnit = parentProduct.MeasureUnit ?? MeasureUnit;
                Weight = parentProduct.Weight ?? Weight;
                WeightUnit = parentProduct.WeightUnit ?? WeightUnit;
                PackageType = parentProduct.PackageType ?? PackageType;
            }
        }

        protected virtual void InheritPropertyValues(CatalogProduct parentProduct)
        {
            // Inherit not overridden property values from main product
            foreach (var parentProductProperty in parentProduct.Properties ?? Array.Empty<Property>())
            {
                Properties ??= new List<Property>();
                var existProperty = Properties.FirstOrDefault(x =>
                    x.IsSame(parentProductProperty, PropertyType.Product, PropertyType.Variation));
                if (existProperty == null)
                {
                    existProperty = AbstractTypeFactory<Property>.TryCreateInstance();
                    Properties.Add(existProperty);
                }

                existProperty.TryInheritFrom(parentProductProperty);
                existProperty.IsReadOnly = existProperty.Type != PropertyType.Variation &&
                                           existProperty.Type != PropertyType.Product;

                // Inherit only parent Product properties  values if own values aren't set
                if (parentProductProperty.Type == PropertyType.Product && existProperty.Values.IsNullOrEmpty() &&
                    !parentProductProperty.Values.IsNullOrEmpty())
                {
                    existProperty.Values = new List<PropertyValue>();
                    foreach (var parentPropValue in parentProductProperty.Values)
                    {
                        var propValue = AbstractTypeFactory<PropertyValue>.TryCreateInstance();
                        propValue.TryInheritFrom(parentPropValue);
                        existProperty.Values.Add(propValue);
                    }
                }
            }
        }

        protected virtual void InheritReviews(CatalogProduct parentProduct)
        {
            // Inherit editorial reviews from main product
            if (Reviews.IsNullOrEmpty() && parentProduct.Reviews != null)
            {
                Reviews = new List<EditorialReview>();
                foreach (var parentReview in parentProduct.Reviews)
                {
                    var review = AbstractTypeFactory<EditorialReview>.TryCreateInstance();
                    review.TryInheritFrom(parentReview);
                    Reviews.Add(review);
                }
            }
        }

        protected virtual void InheritAssets(CatalogProduct parentProduct)
        {
            // Inherit assets from parent product (if not set)
            if (Assets.IsNullOrEmpty() && !parentProduct.Assets.IsNullOrEmpty())
            {
                Assets = new List<Asset>();
                foreach (var parentAsset in parentProduct.Assets)
                {
                    var asset = AbstractTypeFactory<Asset>.TryCreateInstance();
                    asset.TryInheritFrom(parentAsset);
                    Assets.Add(asset);
                }
            }
        }

        protected virtual void InheritImages(CatalogProduct parentProduct)
        {
            // Inherit images from parent product (if not set)
            if (Images.IsNullOrEmpty() && !parentProduct.Images.IsNullOrEmpty())
            {
                Images = new List<Image>();
                foreach (var parentImage in parentProduct.Images)
                {
                    var image = AbstractTypeFactory<Image>.TryCreateInstance();
                    image.TryInheritFrom(parentImage);
                    Images.Add(image);
                }
            }
        }

        protected virtual void InheritTaxes(IEntity parent)
        {
            // TODO: prevent saving the inherited simple values
            // TaxType  inheritance
            if (parent is IHasTaxType hasTaxType && TaxType == null)
            {
                TaxType = hasTaxType.TaxType;
            }
        }

        protected virtual void InheritProperties(IEntity parent)
        {
            if (parent is IHasProperties hasProperties)
            {
                // Properties inheritance
                foreach (var parentProperty in hasProperties.Properties ?? Array.Empty<Property>())
                {
                    if (this.HasPropertyExcluded(parentProperty.Name))
                    {
                        continue;
                    }

                    Properties ??= new List<Property>();
                    var existProperty = Properties.FirstOrDefault(x =>
                        x.IsSame(parentProperty, PropertyType.Product, PropertyType.Variation));
                    if (existProperty == null)
                    {
                        existProperty = AbstractTypeFactory<Property>.TryCreateInstance();
                        Properties.Add(existProperty);
                    }

                    existProperty.TryInheritFrom(parentProperty);

                    existProperty.IsReadOnly = existProperty.Type != PropertyType.Variation &&
                                               existProperty.Type != PropertyType.Product;
                }

                // Restore sorting order after changes
                Properties = Properties?.OrderBy(x => x.Name).ToList();
            }
        }

        #endregion

        public bool ParentCategoryIsActive
        {
            get
            {
                return Category is null ||
                       Category.IsActive == true && Category.ParentIsActive;
            }
        }

        public virtual object GetCopy()
        {
            var result = this.CloneTyped();

            result.Id = null;

            result.Images = Images?.Select(x => x.GetCopy()).OfType<Image>().ToList();
            result.Assets = Assets?.Select(x => x.GetCopy()).OfType<Asset>().ToList();
            result.Properties = Properties?.Select(x => x.GetCopy()).OfType<Property>().ToList();
            result.Variations = Variations?.Select(x => x.GetCopy()).OfType<Variation>().ToList();
            result.Reviews = Reviews?.Select(x => x.GetCopy()).OfType<EditorialReview>().ToList();
            result.Associations = Associations?.Select(x => x.GetCopy()).OfType<ProductAssociation>().ToList();
            // Clear ID for all related entities except properties
            var allSeoSupportEntities = result.GetFlatObjectsListWithInterface<ISeoSupport>();
            foreach (var seoSupportEntity in allSeoSupportEntities)
            {
                seoSupportEntity.SeoInfos?.Clear();
            }
            return result;
        }

        public virtual void Move(string catalogId, string categoryId)
        {
            CatalogId = catalogId;
            CategoryId = categoryId;
            foreach (var variation in Variations)
            {
                variation.CatalogId = catalogId;
                variation.CategoryId = categoryId;
            }
        }

        public virtual void ReduceDetails(string responseGroup)
        {
            // Reduce details according to response group
            var productResponseGroup = EnumUtility.SafeParseFlags(responseGroup, ItemResponseGroup.ItemLarge);

            if (!productResponseGroup.HasFlag(ItemResponseGroup.ItemAssets))
            {
                Assets = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
            {
                Associations = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.ReferencedAssociations))
            {
                ReferencedAssociations = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
            {
                Reviews = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.ItemProperties))
            {
                Properties = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.Links))
            {
                Links = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.Outlines))
            {
                Outlines = null;
            }
            if (!productResponseGroup.HasFlag(ItemResponseGroup.Seo))
            {
                SeoInfos = null;
            }

#pragma warning disable CS0618 // Variations can be used here
            if (!productResponseGroup.HasFlag(ItemResponseGroup.Variations))
            {
                Variations = null;
            }
#pragma warning restore CS0618
        }

        #region ICloneable members
        public virtual object Clone()
        {
            var result = (CatalogProduct)MemberwiseClone();

            //result.Catalog = Catalog?.CloneTyped(); // Intentionally temporary disabled due to memory overhead
            //result.Category = Category?.CloneTyped(); // Intentionally temporary disabled due to memory overhead
            result.SeoInfos = SeoInfos?.Select(x => x.CloneTyped()).ToList();
            result.Images = Images?.Select(x => x.CloneTyped()).ToList();
            result.Assets = Assets?.Select(x => x.CloneTyped()).ToList();
            result.Properties = Properties?.Select(x => x.CloneTyped()).ToList();
            result.Associations = Associations?.Select(x => x.CloneTyped()).ToList();
            result.ReferencedAssociations = ReferencedAssociations?.Select(x => x.CloneTyped()).ToList();
            result.Reviews = Reviews?.Select(x => x.CloneTyped()).ToList();
            result.Links = Links?.Select(x => x.CloneTyped()).ToList();
            result.Variations = Variations?.Select(x => x.CloneTyped()).ToList();
            //result.Outlines = Outlines?.Select(x => x.CloneTyped()).ToList(); // Intentionally temporary disabled due to memory overhead

            return result;
        }
        #endregion

        public override string ToString()
        {
            return $"{Name}, Id: {Id}, Code: {Code}";
        }
    }
}
