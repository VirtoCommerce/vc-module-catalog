using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Common;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
    public static class ItemConverter
    {
        /// <summary>
        /// Converting to model type
        /// </summary>
        /// <returns></returns>
        public static coreModel.CatalogProduct ToCoreModel(this dataModel.ItemEntity dbItem, dataModel.CatalogEntity[] allCatalogs, dataModel.CategoryEntity[] allCategories, bool convertChildrens = true)
        {
            var retVal = new coreModel.CatalogProduct();
            retVal.InjectFrom(dbItem);
            retVal.Catalog = allCatalogs.First(x => x.Id == dbItem.CatalogId).ToCoreModel();

            if (dbItem.CategoryId != null)
            {
                retVal.Category = allCategories.First(x => x.Id == dbItem.CategoryId)
                                               .ToCoreModel(allCatalogs, allCategories);
            }

            retVal.MainProductId = dbItem.ParentId;
            if (dbItem.Parent != null)
            {
                retVal.MainProduct = dbItem.Parent.ToCoreModel(allCatalogs, allCategories, convertChildrens: false);
            }

            retVal.IsActive = dbItem.IsActive;
            retVal.IsBuyable = dbItem.IsBuyable;
            retVal.TrackInventory = dbItem.TrackInventory;

            retVal.MaxQuantity = (int)dbItem.MaxQuantity;
            retVal.MinQuantity = (int)dbItem.MinQuantity;


            //Links
            retVal.Links = dbItem.CategoryLinks.Select(x => x.ToCoreModel(allCatalogs, allCategories)).ToList();

            //Images
            retVal.Images = dbItem.Images.OrderBy(x => x.SortOrder).Select(x => x.ToCoreModel()).ToList();
            //Inherit images from parent product (if its not set)
            if (!retVal.Images.Any() && retVal.MainProduct != null && retVal.MainProduct.Images != null)
            {
                retVal.Images = retVal.MainProduct.Images.Select(x => x.Clone()).OfType<coreModel.Image>().ToList();
                foreach (var image in retVal.Images)
                {
                    image.Id = null;
                    image.IsInherited = true;
                }
            }

            //Assets
            retVal.Assets = dbItem.Assets.OrderBy(x => x.CreatedDate).Select(x => x.ToCoreModel()).ToList();
            //Inherit images from parent product (if its not set)
            if (!retVal.Assets.Any() && retVal.MainProduct != null && retVal.MainProduct.Assets != null)
            {
                retVal.Assets = retVal.MainProduct.Assets.Select(x => x.Clone()).OfType<coreModel.Asset>().ToList();
                foreach (var asset in retVal.Assets)
                {
                    asset.Id = null;
                    asset.IsInherited = true;
                }
            }

            // EditorialReviews
            retVal.Reviews = dbItem.EditorialReviews.Select(x => x.ToCoreModel()).ToList();

            //inherit editorial reviews from main product and do not inherit if variation loaded within product
            if (!retVal.Reviews.Any() && retVal.MainProduct != null && retVal.MainProduct.Reviews != null && convertChildrens)
            {
                retVal.Reviews = retVal.MainProduct.Reviews.Select(x => x.Clone()).OfType<coreModel.EditorialReview>().ToList();
                foreach (var review in retVal.Reviews)
                {
                    review.Id = null;
                    review.IsInherited = true;
                }
            }

            // Associations
            retVal.Associations = dbItem.Associations.Select(x => x.ToCoreModel(allCatalogs, allCategories)).OrderBy(x => x.Priority).ToList();

            //TaxType category inheritance
            if (retVal.TaxType == null && retVal.Category != null)
            {
                retVal.TaxType = retVal.Category.TaxType;
            }

            retVal.Properties = new List<coreModel.Property>();
            //Properties inheritance
            retVal.Properties.AddRange(retVal.Category != null ? retVal.Category.Properties : retVal.Catalog.Properties);
            foreach (var property in retVal.Properties)
            {
                property.IsInherited = true;
            }
            //Sort properties by name
            retVal.Properties = retVal.Properties.OrderBy(x => x.Name).ToList();

            //Self item property values
            retVal.PropertyValues = dbItem.ItemPropertyValues.OrderBy(x => x.Name).Select(x => x.ToCoreModel()).ToList();
            foreach (var propertyValue in retVal.PropertyValues.ToArray())
            {
                //Try to find property meta information
                propertyValue.Property = retVal.Properties.FirstOrDefault(x => x.IsSuitableForValue(propertyValue));
                //Return each localized value for selected dictionary value
                //Because multilingual dictionary values for all languages may not stored in db need add it in result manually from property dictionary values
                var localizedDictValues = propertyValue.TryGetAllLocalizedDictValues();
                foreach (var localizedDictValue in localizedDictValues)
                {
                    if (!retVal.PropertyValues.Any(x => x.ValueId == localizedDictValue.ValueId && x.LanguageCode == localizedDictValue.LanguageCode))
                    {
                        retVal.PropertyValues.Add(localizedDictValue);
                    }
                }
            }

            //inherit not overriden property values from main product
            if (retVal.MainProduct != null && retVal.MainProduct.PropertyValues != null)
            {
                var mainProductPopValuesGroups = retVal.MainProduct.PropertyValues.GroupBy(x => x.PropertyName);
                foreach (var group in mainProductPopValuesGroups)
                {
                    //Inherit all values if not overriden
                    if (!retVal.PropertyValues.Any(x => x.PropertyName.EqualsInvariant(group.Key)))
                    {
                        foreach (var inheritedpropValue in group)
                        {
                            inheritedpropValue.Id = null;
                            inheritedpropValue.IsInherited = true;
                            retVal.PropertyValues.Add(inheritedpropValue);
                        }
                    }
                }
            }

            if (convertChildrens)
            {
                // Variations
                retVal.Variations = new List<coreModel.CatalogProduct>();
                foreach (var variation in dbItem.Childrens)
                {
                    var productVariation = variation.ToCoreModel(allCatalogs, allCategories, convertChildrens: false);
                    productVariation.MainProduct = retVal;
                    productVariation.MainProductId = retVal.Id;

                    retVal.Variations.Add(productVariation);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Converting to foundation type
        /// </summary>
        /// <param name="product"></param>
        /// <param name="pkMap"></param>
        /// <returns></returns>
        public static dataModel.ItemEntity ToDataModel(this coreModel.CatalogProduct product, PrimaryKeyResolvingMap pkMap)
        {
            var retVal = new dataModel.ItemEntity();
            pkMap.AddPair(product, retVal);
            retVal.InjectFrom(product);

            if (product.StartDate == default(DateTime))
            {
                retVal.StartDate = DateTime.UtcNow;
            }

            retVal.IsActive = product.IsActive ?? true;
            retVal.IsBuyable = product.IsBuyable ?? true;
            retVal.TrackInventory = product.TrackInventory ?? true;
            retVal.MaxQuantity = product.MaxQuantity ?? 0;
            retVal.MinQuantity = product.MinQuantity ?? 0;

            retVal.ParentId = product.MainProductId;
            //Constant fields
            //Only for main product
            retVal.AvailabilityRule = (int)coreModel.AvailabilityRule.Always;
            retVal.MinQuantity = 1;
            retVal.MaxQuantity = 0;

            retVal.CatalogId = product.CatalogId;
            retVal.CategoryId = string.IsNullOrEmpty(product.CategoryId) ? null : product.CategoryId;

            #region ItemPropertyValues
            if (product.PropertyValues != null)
            {
                retVal.ItemPropertyValues = new ObservableCollection<dataModel.PropertyValueEntity>();
                foreach(var propertyValue in product.PropertyValues)
                {
                    if(!propertyValue.IsInherited && propertyValue.Value != null && !string.IsNullOrEmpty(propertyValue.Value.ToString()))
                    {
                        var dbPropertyValue = propertyValue.ToDataModel(pkMap);
                        retVal.ItemPropertyValues.Add(dbPropertyValue);
                    }
                }              
            }
            #endregion

            #region Assets
            if (product.Assets != null)
            {
                retVal.Assets = new ObservableCollection<dataModel.AssetEntity>(product.Assets.Where(x => !x.IsInherited).Select(x => x.ToDataModel(pkMap)));
            }
            #endregion

            #region Images
            if (product.Images != null)
            {
                retVal.Images = new ObservableCollection<dataModel.ImageEntity>(product.Images.Where(x => !x.IsInherited).Select(x => x.ToDataModel(pkMap)));
            }
            #endregion

            #region Links
            if (product.Links != null)
            {
                retVal.CategoryLinks = new ObservableCollection<dataModel.CategoryItemRelationEntity>();
                retVal.CategoryLinks.AddRange(product.Links.Select(x => x.ToDataModel(product)));
            }
            #endregion

            #region EditorialReview
            if (product.Reviews != null)
            {
                retVal.EditorialReviews = new ObservableCollection<dataModel.EditorialReviewEntity>();
                retVal.EditorialReviews.AddRange(product.Reviews.Where(x => !x.IsInherited).Select(x => x.ToDataModel(retVal, pkMap)));
            }
            #endregion

            #region Associations
            if (product.Associations != null)
            {
                retVal.Associations = new ObservableCollection<dataModel.AssociationEntity>(product.Associations.Select(x => x.ToDataModel()));
            }
            #endregion

            return retVal;
        }

        /// <summary>
        /// Patch changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="pkMap"></param>
        public static void Patch(this coreModel.CatalogProduct source, dataModel.ItemEntity target, PrimaryKeyResolvingMap pkMap)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            // TODO: temporary solution because partial update replaced not nullable properties in db entity
            if (source.IsBuyable != null)
                target.IsBuyable = source.IsBuyable.Value;

            if (source.IsActive != null)
                target.IsActive = source.IsActive.Value;

            if (source.TrackInventory != null)
                target.TrackInventory = source.TrackInventory.Value;

            if (source.MinQuantity != null)
                target.MinQuantity = source.MinQuantity.Value;

            if (source.MaxQuantity != null)
                target.MaxQuantity = source.MaxQuantity.Value;

            if (source.EnableReview != null)
                target.EnableReview = source.EnableReview.Value;

            target.CatalogId = string.IsNullOrEmpty(source.CatalogId) ? null : source.CatalogId;
            target.CategoryId = string.IsNullOrEmpty(source.CategoryId) ? null : source.CategoryId;
            target.Name = source.Name;
            target.Code = source.Code;
            target.ManufacturerPartNumber = source.ManufacturerPartNumber;
            target.Gtin = source.Gtin;
            target.ProductType = source.ProductType;
            target.MaxNumberOfDownload = source.MaxNumberOfDownload;
            target.DownloadType = source.DownloadType;
            target.HasUserAgreement = source.HasUserAgreement;
            target.DownloadExpiration = source.DownloadExpiration;
            target.Vendor = source.Vendor;
            target.TaxType = source.TaxType;
            target.WeightUnit = source.WeightUnit;
            target.Weight = source.Weight;
            target.MeasureUnit = source.MeasureUnit;
            target.PackageType = source.PackageType;
            target.Height = source.Height;
            target.Length = source.Length;
            target.Width = source.Width;
            target.ShippingType = source.ShippingType;
            target.Priority = source.Priority;
            target.ParentId = source.MainProductId;

            var dbSource = source.ToDataModel(pkMap);

            #region Assets
            if (!dbSource.Assets.IsNullCollection())
            {
                dbSource.Assets.Patch(target.Assets, (sourceAsset, targetAsset) => sourceAsset.Patch(targetAsset));
            }
            #endregion

            #region Images
            if (!dbSource.Images.IsNullCollection())
            {
                dbSource.Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }
            #endregion

            #region ItemPropertyValues
            if (!dbSource.ItemPropertyValues.IsNullCollection())
            {
                dbSource.ItemPropertyValues.Patch(target.ItemPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Links
            if (!dbSource.CategoryLinks.IsNullCollection())
            {
                dbSource.CategoryLinks.Patch(target.CategoryLinks, new CategoryItemRelationComparer(),
                                         (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region EditorialReviews
            if (!dbSource.EditorialReviews.IsNullCollection())
            {
                dbSource.EditorialReviews.Patch(target.EditorialReviews, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            #endregion

            #region Association
            if (!dbSource.Associations.IsNullCollection())
            {
                var associationComparer = AnonymousComparer.Create((dataModel.AssociationEntity x) => x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                dbSource.Associations.Patch(target.Associations, associationComparer,
                                             (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));
            }
            #endregion
        }
    }
}
