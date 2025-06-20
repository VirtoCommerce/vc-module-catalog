using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class ItemEntity : AuditableEntity, IHasOuterId, IDataEntity<ItemEntity, CatalogProduct>
    {
        [StringLength(1024)]
        [Required]
        public string Name { get; set; }

        public ObservableCollection<ItemLocalizedNameEntity> LocalizedNames { get; set; }
            = new NullCollection<ItemLocalizedNameEntity>();

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public bool IsBuyable { get; set; }

        public int AvailabilityRule { get; set; }

        public decimal MinQuantity { get; set; }

        public decimal MaxQuantity { get; set; }

        public int PackSize { get; set; }

        public bool TrackInventory { get; set; }

        [StringLength(128)]
        public string PackageType { get; set; }

        [StringLength(64)]
        [Required]
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

        [StringLength(128)]
        public string OuterId { get; set; }

        #region Navigation Properties

        public string CatalogId { get; set; }
        public virtual CatalogEntity Catalog { get; set; }

        public string CategoryId { get; set; }
        public virtual CategoryEntity Category { get; set; }

        public string ParentId { get; set; }
        public virtual ItemEntity Parent { get; set; }

        public virtual ObservableCollection<CategoryItemRelationEntity> CategoryLinks { get; set; }
            = new NullCollection<CategoryItemRelationEntity>();

        public virtual ObservableCollection<AssetEntity> Assets { get; set; }
            = new NullCollection<AssetEntity>();

        public virtual ObservableCollection<ImageEntity> Images { get; set; }
            = new NullCollection<ImageEntity>();

        public virtual ObservableCollection<AssociationEntity> Associations { get; set; }
            = new NullCollection<AssociationEntity>();

        public virtual ObservableCollection<AssociationEntity> ReferencedAssociations { get; set; }
            = new NullCollection<AssociationEntity>();

        public virtual ObservableCollection<EditorialReviewEntity> EditorialReviews { get; set; }
            = new NullCollection<EditorialReviewEntity>();

        public virtual ObservableCollection<PropertyValueEntity> ItemPropertyValues { get; set; }
            = new NullCollection<PropertyValueEntity>();

        public virtual ObservableCollection<SeoInfoEntity> SeoInfos { get; set; }
            = new NullCollection<SeoInfoEntity>();

        public virtual ObservableCollection<ItemEntity> Childrens { get; set; }
            = new NullCollection<ItemEntity>();

        #endregion

        public virtual CatalogProduct ToModel(CatalogProduct product)
        {
            return ToModel(product, convertChildrens: true, convertAssociations: true);
        }

        public virtual CatalogProduct ToModel(CatalogProduct product, bool convertChildrens, bool convertAssociations)
        {
            ArgumentNullException.ThrowIfNull(product);

            product.Id = Id;
            product.CreatedDate = CreatedDate;
            product.CreatedBy = CreatedBy;
            product.ModifiedDate = ModifiedDate;
            product.ModifiedBy = ModifiedBy;
            product.OuterId = OuterId;

            product.CatalogId = CatalogId;
            product.CategoryId = CategoryId;
            product.Code = Code;
            product.DownloadExpiration = DownloadExpiration;
            product.DownloadType = DownloadType;
            product.EnableReview = EnableReview;
            product.EndDate = EndDate;
            product.Gtin = Gtin;
            product.HasUserAgreement = HasUserAgreement;
            product.Height = Height;
            product.IsActive = IsActive;
            product.IsBuyable = IsBuyable;
            product.Length = Length;
            product.MainProductId = ParentId;
            product.ManufacturerPartNumber = ManufacturerPartNumber;
            product.MaxNumberOfDownload = MaxNumberOfDownload;
            product.MaxQuantity = (int)MaxQuantity;
            product.MeasureUnit = MeasureUnit;
            product.MinQuantity = (int)MinQuantity;
            product.PackSize = PackSize >= 1 ? PackSize : 1;
            product.Name = Name;
            product.PackageType = PackageType;
            product.Priority = Priority;
            product.ProductType = ProductType;
            product.ShippingType = ShippingType;
            product.StartDate = StartDate;
            product.TaxType = TaxType;
            product.TrackInventory = TrackInventory;
            product.Vendor = Vendor;
            product.Weight = Weight;
            product.WeightUnit = WeightUnit;
            product.Width = Width;
            product.Links = CategoryLinks.Select(x => x.ToModel(AbstractTypeFactory<CategoryLink>.TryCreateInstance())).ToList();
            product.Images = Images.OrderBy(x => x.SortOrder).Select(x => x.ToModel(AbstractTypeFactory<Image>.TryCreateInstance())).ToList();
            product.Assets = Assets.OrderBy(x => x.CreatedDate).Select(x => x.ToModel(AbstractTypeFactory<Asset>.TryCreateInstance())).ToList();
            product.Reviews = EditorialReviews.Select(x => x.ToModel(AbstractTypeFactory<EditorialReview>.TryCreateInstance())).ToList();
            product.SeoInfos = SeoInfos.Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();

            if (LocalizedNames != null)
            {
                product.LocalizedName = new LocalizedString();
                foreach (var localizedName in LocalizedNames)
                {
                    product.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
                }
            }

            product.Properties = new List<Property>();
            if (convertAssociations)
            {
                // Associations
                product.Associations = Associations.Select(x => x.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())).OrderBy(x => x.Priority).ToList();
                product.ReferencedAssociations = ReferencedAssociations.Select(x => x.ToReferencedAssociationModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())).OrderBy(x => x.Priority).ToList();
            }

            if (!ItemPropertyValues.IsNullOrEmpty())
            {
                var propertyValues = ItemPropertyValues.OrderBy(x => x.DictionaryItem?.SortOrder)
                                                       .ThenBy(x => x.Name)
                                                       .SelectMany(pv => pv.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance()).ToList());

                product.Properties = propertyValues.GroupBy(pv => pv.PropertyName).Select(values =>
                {
                    var property = AbstractTypeFactory<Property>.TryCreateInstance();
                    property.Name = values.Key;
                    property.ValueType = values.First().ValueType;
                    property.Values = values.ToList();
                    foreach (var propValue in property.Values)
                    {
                        propValue.Property = property;
                    }
                    return property;
                }).OrderBy(x => x.Name).ToList();
            }

            if (Parent != null)
            {
                product.MainProduct = Parent.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, convertAssociations);
            }

            if (convertChildrens)
            {
                // Variations
                product.Variations = new List<Variation>();
                foreach (var variation in Childrens)
                {
                    var productVariation = (Variation)variation.ToModel(AbstractTypeFactory<Variation>.TryCreateInstance());
                    productVariation.MainProduct = product;
                    productVariation.MainProductId = product.Id;
                    product.Variations.Add(productVariation);
                }
            }

            return product;
        }

        public virtual ItemEntity FromModel(CatalogProduct product, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(product);

            pkMap.AddPair(product, this);

            Id = product.Id;
            CreatedDate = product.CreatedDate;
            CreatedBy = product.CreatedBy;
            ModifiedDate = product.ModifiedDate;
            ModifiedBy = product.ModifiedBy;
            OuterId = product.OuterId;

            CatalogId = product.CatalogId;
            CategoryId = product.CategoryId;
            Code = product.Code;
            DownloadExpiration = product.DownloadExpiration;
            DownloadType = product.DownloadType;
            EnableReview = product.EnableReview;
            EndDate = product.EndDate;
            Gtin = product.Gtin;
            HasUserAgreement = product.HasUserAgreement;
            Height = product.Height;
            IsActive = product.IsActive ?? true;
            IsBuyable = product.IsBuyable ?? true;
            Length = product.Length;
            ParentId = product.MainProductId;
            ManufacturerPartNumber = product.ManufacturerPartNumber;
            MaxNumberOfDownload = product.MaxNumberOfDownload;
            MaxQuantity = product.MaxQuantity ?? 0;
            MeasureUnit = product.MeasureUnit;
            MinQuantity = product.MinQuantity ?? 0;
            PackSize = product.PackSize >= 1 ? product.PackSize : 1;
            Name = product.Name;
            PackageType = product.PackageType;
            Priority = product.Priority;
            ProductType = product.ProductType;
            ShippingType = product.ShippingType;
            TaxType = product.TaxType;
            TrackInventory = product.TrackInventory ?? true;
            Vendor = product.Vendor;
            Weight = product.Weight;
            WeightUnit = product.WeightUnit;
            Width = product.Width;

            StartDate = product.StartDate == default ? DateTime.UtcNow : product.StartDate;

            // Constant fields
            // Only for main product
            AvailabilityRule = (int)Core.Model.AvailabilityRule.Always;

            CatalogId = product.CatalogId;
            CategoryId = string.IsNullOrEmpty(product.CategoryId) ? null : product.CategoryId;

            #region ItemPropertyValues
            if (!product.Properties.IsNullOrEmpty())
            {
                var propValues = new List<PropertyValue>();
                var forceSavePropertyValues = false;

                foreach (var property in product.Properties.Where(x => x.Type == PropertyType.Product || x.Type == PropertyType.Variation))
                {
                    if (property.Values != null)
                    {
                        // Do not use values from inherited properties
                        var propertyValues = property.Values.Where(pv => pv != null && !pv.IsInherited).ToList();

                        if (propertyValues.Any())
                        {
                            foreach (var propValue in propertyValues)
                            {
                                // Need populate required fields
                                propValue.PropertyName = property.Name;
                                propValue.ValueType = property.ValueType;
                                propValues.Add(propValue);
                            }
                        }
                        else
                        {
                            // Set true to be able to remove property values from database
                            forceSavePropertyValues = true;
                        }
                    }
                }

                if (forceSavePropertyValues || propValues.Any())
                {
                    ItemPropertyValues = new ObservableCollection<PropertyValueEntity>(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModels(propValues, pkMap));
                }
            }

#pragma warning disable CS0618 // PropertyValues can be used here for backward compatibility
            else if (!product.PropertyValues.IsNullOrEmpty())
            {
                // Backward compatibility
                // TODO: Remove later
                ItemPropertyValues = new ObservableCollection<PropertyValueEntity>(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModels(product.PropertyValues, pkMap));
            }
#pragma warning restore CS0618
            #endregion

            #region Assets
            if (product.Assets != null)
            {
                Assets = new ObservableCollection<AssetEntity>(product.Assets.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<AssetEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Images
            if (product.Images != null)
            {
                Images = new ObservableCollection<ImageEntity>(product.Images.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<ImageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Links
            if (product.Links != null)
            {
                CategoryLinks = new ObservableCollection<CategoryItemRelationEntity>(product.Links.Select(x => AbstractTypeFactory<CategoryItemRelationEntity>.TryCreateInstance().FromModel(x)));
            }
            #endregion

            #region EditorialReview
            if (product.Reviews != null)
            {
                EditorialReviews = new ObservableCollection<EditorialReviewEntity>(product.Reviews.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<EditorialReviewEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            #region Associations
            if (product.Associations != null)
            {
                Associations = new ObservableCollection<AssociationEntity>(product.Associations.Select(x => AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(x)));
            }
            #endregion

            #region SeoInfo
            if (product.SeoInfos != null)
            {
                SeoInfos = new ObservableCollection<SeoInfoEntity>(product.SeoInfos.Select(x => AbstractTypeFactory<SeoInfoEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            #endregion

            if (product.Variations != null)
            {
                Childrens = new ObservableCollection<ItemEntity>(product.Variations.Select(x => AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (product.LocalizedName != null)
            {
                LocalizedNames = new ObservableCollection<ItemLocalizedNameEntity>(product.LocalizedName.Values
                    .Select(x =>
                    {
                        var entity = AbstractTypeFactory<ItemLocalizedNameEntity>.TryCreateInstance();
                        entity.LanguageCode = x.Key;
                        entity.Value = x.Value;
                        return entity;
                    }));
            }

            return this;
        }

        public virtual void Patch(ItemEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.IsBuyable = IsBuyable;
            target.IsActive = IsActive;
            target.TrackInventory = TrackInventory;
            target.MinQuantity = MinQuantity;
            target.MaxQuantity = MaxQuantity;
            target.PackSize = PackSize;
            target.EnableReview = EnableReview;
            target.OuterId = OuterId;
            target.CatalogId = CatalogId;
            target.CategoryId = CategoryId;
            target.Name = Name;
            target.Code = Code;
            target.ManufacturerPartNumber = ManufacturerPartNumber;
            target.Gtin = Gtin;
            target.ProductType = ProductType;
            target.MaxNumberOfDownload = MaxNumberOfDownload;
            target.DownloadType = DownloadType;
            target.HasUserAgreement = HasUserAgreement;
            target.DownloadExpiration = DownloadExpiration;
            target.Vendor = Vendor;
            target.TaxType = TaxType;
            target.WeightUnit = WeightUnit;
            target.Weight = Weight;
            target.MeasureUnit = MeasureUnit;
            target.PackageType = PackageType;
            target.Height = Height;
            target.Length = Length;
            target.Width = Width;
            target.ShippingType = ShippingType;
            target.Priority = Priority;
            target.ParentId = ParentId;
            target.StartDate = StartDate;
            target.EndDate = EndDate;

            #region Assets
            if (!Assets.IsNullCollection())
            {
                Assets.Patch(target.Assets, (sourceAsset, targetAsset) => sourceAsset.Patch(targetAsset));
            }
            #endregion

            #region Images
            if (!Images.IsNullCollection())
            {
                Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }
            #endregion

            #region ItemPropertyValues
            if (!ItemPropertyValues.IsNullCollection())
            {
                ItemPropertyValues.Patch(target.ItemPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Links
            if (!CategoryLinks.IsNullCollection())
            {
                var categoryItemRelationComparer = AnonymousComparer.Create((CategoryItemRelationEntity x) => string.Join(":", x.CatalogId, x.CategoryId));
                CategoryLinks.Patch(target.CategoryLinks, categoryItemRelationComparer,
                                         (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region EditorialReviews
            if (!EditorialReviews.IsNullCollection())
            {
                EditorialReviews.Patch(target.EditorialReviews, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Association
            if (!Associations.IsNullCollection())
            {
                Associations.Patch(target.Associations, new AssociationEntityComparer(), (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region SeoInfos
            if (!SeoInfos.IsNullCollection())
            {
                SeoInfos.Patch(target.SeoInfos, (sourceSeoInfo, targetSeoInfo) => sourceSeoInfo.Patch(targetSeoInfo));
            }
            #endregion

            #region Childrens
            if (!Childrens.IsNullCollection())
            {
                Childrens.Patch(target.Childrens, (sourceChildren, targetChildren) => sourceChildren.Patch(targetChildren));
            }
            #endregion

            #region LocalizedName
            if (!LocalizedNames.IsNullCollection())
            {
                var localizedNameComparer = AnonymousComparer.Create((ItemLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
                LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (sourceDisplayName, targetDisplayName) => { });
            }
            #endregion
        }
    }
}
