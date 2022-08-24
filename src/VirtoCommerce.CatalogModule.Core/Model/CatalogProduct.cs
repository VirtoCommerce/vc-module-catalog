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
        /// SKU code
        /// </summary>
        public string Code { get; set; }
        public string ManufacturerPartNumber { get; set; }
        /// <summary>
        /// Global Trade Item Number (GTIN). These identifiers include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).
        /// </summary>
        public string Gtin { get; set; }
        public string Name { get; set; }

        public string CatalogId { get; set; }
        [JsonIgnore]
        public Catalog Catalog { get; set; }

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
        public string MainProductId { get; set; }
        [JsonIgnore]
        public CatalogProduct MainProduct { get; set; }
        public bool? IsBuyable { get; set; }
        public bool? IsActive { get; set; }
        public bool? TrackInventory { get; set; }
        public DateTime? IndexingDate { get; set; }
        public int? MaxQuantity { get; set; }
        public int? MinQuantity { get; set; }

        /// <summary>
        /// Can be Physical, Digital or Subscription.
        /// </summary>
        public string ProductType { get; set; }

        // Type of product package (set of package types with their specific dimensions)
        public string PackageType { get; set; }

        #region IHaveDimension Members
        public string WeightUnit { get; set; }
        public decimal? Weight { get; set; }

        public string MeasureUnit { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        #endregion

        public bool? EnableReview { get; set; }



        /// <summary>
        /// re-downloads limit
        /// </summary>
        public int? MaxNumberOfDownload { get; set; }
        public DateTime? DownloadExpiration { get; set; }
        /// <summary>
        /// DownloadType: {Standard Product, Software, Music}
        /// </summary>
        public string DownloadType { get; set; }
        public bool? HasUserAgreement { get; set; }

        public string ShippingType { get; set; }
        public string TaxType { get; set; }

        public string Vendor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Product order position in catalog
        /// </summary>
        public int Priority { get; set; }

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
            // Inherit editorial reviews from main product and do not inherit if variation loaded within product
            var isVariation = GetType().IsAssignableFrom(typeof(Variation));
            if (!isVariation && Reviews.IsNullOrEmpty() && parentProduct.Reviews != null)
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
