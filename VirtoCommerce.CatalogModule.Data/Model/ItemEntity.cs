using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class ItemEntity : AuditableEntity
    {
        public ItemEntity()
        {
            CategoryLinks = new NullCollection<CategoryItemRelationEntity>();
            Images = new NullCollection<ImageEntity>();
            Assets = new NullCollection<AssetEntity>();
            EditorialReviews = new NullCollection<EditorialReviewEntity>();
            ItemPropertyValues = new NullCollection<PropertyValueEntity>();
            Childrens = new NullCollection<ItemEntity>();
            Associations = new NullCollection<AssociationEntity>();
            ReferencedAssociations = new NullCollection<AssociationEntity>();
        }

        [StringLength(1024)]
        [Required]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public bool IsBuyable { get; set; }

        public int AvailabilityRule { get; set; }

        public decimal MinQuantity { get; set; }

        public decimal MaxQuantity { get; set; }

        public bool TrackInventory { get; set; }


        [StringLength(128)]
        public string PackageType { get; set; }

        [StringLength(64)]
        [Required]
        [Index(IsUnique = true)]
        public string Code { get; set; }

        [StringLength(128)]
        public string ManufacturerPartNumber { get; set; }
        [StringLength(64)]
        public string Gtin { get; set; }

        [StringLength(64)]
        public string ProductType { get; set; }

        [StringLength(32)]
        public string WeightUnit { get; set; }
        public decimal? Weight { get; set; }
        [StringLength(32)]
        public string MeasureUnit { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }

        public bool? EnableReview { get; set; }

        public int? MaxNumberOfDownload { get; set; }
        public DateTime? DownloadExpiration { get; set; }
        [StringLength(64)]
        public string DownloadType { get; set; }
        public bool? HasUserAgreement { get; set; }
        [StringLength(64)]
        public string ShippingType { get; set; }
        [StringLength(64)]
        public string TaxType { get; set; }
        [StringLength(128)]
        public string Vendor { get; set; }

        public int Priority { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<CategoryItemRelationEntity> CategoryLinks { get; set; }

        public virtual ObservableCollection<AssetEntity> Assets { get; set; }

        public virtual ObservableCollection<ImageEntity> Images { get; set; }

        public virtual ObservableCollection<AssociationEntity> Associations { get; set; }

        public virtual ObservableCollection<AssociationEntity> ReferencedAssociations { get; set; }

        public virtual ObservableCollection<EditorialReviewEntity> EditorialReviews { get; set; }

        public virtual ObservableCollection<PropertyValueEntity> ItemPropertyValues { get; set; }

        [Index("CatalogIdAndParentId", 1)]
        public string CatalogId { get; set; }
        public virtual CatalogEntity Catalog { get; set; }

        public string CategoryId { get; set; }
        public virtual CategoryEntity Category { get; set; }

        [Index("CatalogIdAndParentId", 2)]
        public string ParentId { get; set; }
        public virtual ItemEntity Parent { get; set; }

        public virtual ObservableCollection<ItemEntity> Childrens { get; set; }
        #endregion


        public virtual CatalogProduct ToModel(CatalogProduct product, bool convertChildrens = true, bool convertAssociations = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));


            product.Id = this.Id;
            product.CreatedDate = this.CreatedDate;
            product.CreatedBy = this.CreatedBy;
            product.ModifiedDate = this.ModifiedDate;
            product.ModifiedBy = this.ModifiedBy;

            product.CatalogId = this.CatalogId;
            product.CategoryId = this.CategoryId;
            product.Code = this.Code;
            product.DownloadExpiration = this.DownloadExpiration;
            product.DownloadType = this.DownloadType;
            product.EnableReview = this.EnableReview;
            product.EndDate = this.EndDate;
            product.Gtin = this.Gtin;
            product.HasUserAgreement = this.HasUserAgreement;
            product.Height = this.Height;
            product.IsActive = this.IsActive;
            product.IsBuyable = this.IsBuyable;
            product.Length = this.Length;
            product.MainProductId = this.ParentId;
            product.ManufacturerPartNumber = this.ManufacturerPartNumber;
            product.MaxNumberOfDownload = this.MaxNumberOfDownload;
            product.MaxQuantity = (int)this.MaxQuantity;
            product.MeasureUnit = this.MeasureUnit;
            product.MinQuantity = (int)this.MinQuantity;
            product.Name = this.Name;
            product.PackageType = this.PackageType;
            product.Priority = this.Priority;
            product.ProductType = this.ProductType;
            product.ShippingType = this.ShippingType;
            product.StartDate = this.StartDate;
            product.TaxType = this.TaxType;
            product.TrackInventory = this.TrackInventory;
            product.Vendor = this.Vendor;
            product.Weight = this.Weight;
            product.WeightUnit = this.WeightUnit;
            product.Width = this.Width;

            //Links
            product.Links = this.CategoryLinks.Select(x => x.ToModel(AbstractTypeFactory<CategoryLink>.TryCreateInstance())).ToList();
            //Images
            product.Images = this.Images.OrderBy(x => x.SortOrder).Select(x => x.ToModel(AbstractTypeFactory<Image>.TryCreateInstance())).ToList();
            //Assets
            product.Assets = this.Assets.OrderBy(x => x.CreatedDate).Select(x => x.ToModel(AbstractTypeFactory<Asset>.TryCreateInstance())).ToList();
            // EditorialReviews
            product.Reviews = this.EditorialReviews.Select(x => x.ToModel(AbstractTypeFactory<EditorialReview>.TryCreateInstance())).ToList();

            if (convertAssociations)
            {
                // Associations
                product.Associations = this.Associations.Select(x => x.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())).OrderBy(x => x.Priority).ToList();
                product.ReferencedAssociations = this.ReferencedAssociations.Select(x => x.ToReferencedAssociationModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())).OrderBy(x => x.Priority).ToList();
            }

            //Self item property values
            product.PropertyValues = this.ItemPropertyValues.OrderBy(x => x.Name).Select(x => x.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance())).ToList();

            if (this.Parent != null)
            {
                product.MainProduct = this.Parent.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, convertAssociations);
            }

            if (convertChildrens)
            {
                // Variations
                product.Variations = new List<CatalogProduct>();
                foreach (var variation in this.Childrens)
                {
                    var productVariation = variation.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance());
                    productVariation.MainProduct = product;
                    productVariation.MainProductId = product.Id;
                    product.Variations.Add(productVariation);
                }
            }
            return product;
        }

        public virtual ItemEntity FromModel(CatalogProduct product, PrimaryKeyResolvingMap pkMap)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            pkMap.AddPair(product, this);

            this.Id = product.Id;
            this.CreatedDate = product.CreatedDate;
            this.CreatedBy = product.CreatedBy;
            this.ModifiedDate = product.ModifiedDate;
            this.ModifiedBy = product.ModifiedBy;

            this.CatalogId = product.CatalogId;
            this.CategoryId = product.CategoryId;
            this.Code = product.Code;
            this.DownloadExpiration = product.DownloadExpiration;
            this.DownloadType = product.DownloadType;
            this.EnableReview = product.EnableReview;
            this.EndDate = product.EndDate;
            this.Gtin = product.Gtin;
            this.HasUserAgreement = product.HasUserAgreement;
            this.Height = product.Height;
            this.IsActive = product.IsActive ?? true;
            this.IsBuyable = product.IsBuyable ?? true;
            this.Length = product.Length;
            this.ParentId = product.MainProductId;
            this.ManufacturerPartNumber = product.ManufacturerPartNumber;
            this.MaxNumberOfDownload = product.MaxNumberOfDownload;
            this.MaxQuantity = product.MaxQuantity ?? 0;
            this.MeasureUnit = product.MeasureUnit;
            this.MinQuantity = product.MinQuantity ?? 0;
            this.Name = product.Name;
            this.PackageType = product.PackageType;
            this.Priority = product.Priority;
            this.ProductType = product.ProductType;
            this.ShippingType = product.ShippingType;
            this.TaxType = product.TaxType;
            this.TrackInventory = product.TrackInventory ?? true;
            this.Vendor = product.Vendor;
            this.Weight = product.Weight;
            this.WeightUnit = product.WeightUnit;
            this.Width = product.Width;

            this.StartDate = product.StartDate == default(DateTime) ? DateTime.UtcNow : product.StartDate;

            //Constant fields
            //Only for main product
            this.AvailabilityRule = (int)VirtoCommerce.Domain.Catalog.Model.AvailabilityRule.Always;

            this.CatalogId = product.CatalogId;
            this.CategoryId = string.IsNullOrEmpty(product.CategoryId) ? null : product.CategoryId;

            #region ItemPropertyValues
            if (product.PropertyValues != null)
            {
                this.ItemPropertyValues = new ObservableCollection<PropertyValueEntity>();
                foreach (var propertyValue in product.PropertyValues)
                {
                    if (!propertyValue.IsInherited && propertyValue.Value != null && !string.IsNullOrEmpty(propertyValue.Value.ToString()))
                    {
                        this.ItemPropertyValues.Add(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModel(propertyValue, pkMap));
                    }
                }
            }
            #endregion

            #region Assets
            if (product.Assets != null)
            {
                this.Assets = new ObservableCollection<AssetEntity>(product.Assets.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<AssetEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Images
            if (product.Images != null)
            {
                this.Images = new ObservableCollection<ImageEntity>(product.Images.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<ImageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Links
            if (product.Links != null)
            {
                this.CategoryLinks = new ObservableCollection<CategoryItemRelationEntity>(product.Links.Select(x => AbstractTypeFactory<CategoryItemRelationEntity>.TryCreateInstance().FromModel(x)));
            }
            #endregion

            #region EditorialReview
            if (product.Reviews != null)
            {
                this.EditorialReviews = new ObservableCollection<EditorialReviewEntity>(product.Reviews.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<EditorialReviewEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Associations
            if (product.Associations != null)
            {
                this.Associations = new ObservableCollection<AssociationEntity>(product.Associations.Select(x => AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(x)));
            }
            #endregion

            if (product.Variations != null)
            {
                this.Childrens = new ObservableCollection<ItemEntity>(product.Variations.Select(x => AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(ItemEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.IsBuyable = this.IsBuyable;
            target.IsActive = this.IsActive;
            target.TrackInventory = this.TrackInventory;
            target.MinQuantity = this.MinQuantity;
            target.MaxQuantity = this.MaxQuantity;
            target.EnableReview = this.EnableReview;

            target.CatalogId = this.CatalogId;
            target.CategoryId = this.CategoryId;
            target.Name = this.Name;
            target.Code = this.Code;
            target.ManufacturerPartNumber = this.ManufacturerPartNumber;
            target.Gtin = this.Gtin;
            target.ProductType = this.ProductType;
            target.MaxNumberOfDownload = this.MaxNumberOfDownload;
            target.DownloadType = this.DownloadType;
            target.HasUserAgreement = this.HasUserAgreement;
            target.DownloadExpiration = this.DownloadExpiration;
            target.Vendor = this.Vendor;
            target.TaxType = this.TaxType;
            target.WeightUnit = this.WeightUnit;
            target.Weight = this.Weight;
            target.MeasureUnit = this.MeasureUnit;
            target.PackageType = this.PackageType;
            target.Height = this.Height;
            target.Length = this.Length;
            target.Width = this.Width;
            target.ShippingType = this.ShippingType;
            target.Priority = this.Priority;
            target.ParentId = this.ParentId;

            #region Assets
            if (!this.Assets.IsNullCollection())
            {
                this.Assets.Patch(target.Assets, (sourceAsset, targetAsset) => sourceAsset.Patch(targetAsset));
            }
            #endregion

            #region Images
            if (!this.Images.IsNullCollection())
            {
                this.Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }
            #endregion

            #region ItemPropertyValues
            if (!this.ItemPropertyValues.IsNullCollection())
            {
                this.ItemPropertyValues.Patch(target.ItemPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Links
            if (!this.CategoryLinks.IsNullCollection())
            {
                this.CategoryLinks.Patch(target.CategoryLinks, new CategoryItemRelationComparer(),
                                         (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region EditorialReviews
            if (!this.EditorialReviews.IsNullCollection())
            {
                this.EditorialReviews.Patch(target.EditorialReviews, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Association
            if (!this.Associations.IsNullCollection())
            {
                var associationComparer = AnonymousComparer.Create((AssociationEntity x) => x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                this.Associations.Patch(target.Associations, associationComparer,
                                             (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));
            }
            #endregion
        }
    }
}
