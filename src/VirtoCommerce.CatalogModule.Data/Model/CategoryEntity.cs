using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CategoryEntity : AuditableEntity, IHasOuterId, IHasCatalogId, IDataEntity<CategoryEntity, Category>
    {
        [Required]
        [StringLength(64)]
        public string Code { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public int Priority { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public ObservableCollection<CategoryLocalizedNameEntity> LocalizedNames { get; set; }
            = new NullCollection<CategoryLocalizedNameEntity>();

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        public bool? EnableDescription { get; set; }

        [NotMapped]
        public CategoryEntity[] AllParents
        {
            get
            {
                var retVal = Array.Empty<CategoryEntity>();
                if (ParentCategory != null)
                {
                    retVal = ParentCategory.AllParents.Concat(new[] { ParentCategory }).ToArray();
                }
                return retVal;
            }
        }

        #region Navigation Properties

        [StringLength(128)]
        [ForeignKey("Catalog")]
        [Required]
        public string CatalogId { get; set; }

        public virtual CatalogEntity Catalog { get; set; }

        [StringLength(128)]
        [ForeignKey("ParentCategory")]
        public string ParentCategoryId { get; set; }

        public virtual CategoryEntity ParentCategory { get; set; }

        public virtual ObservableCollection<ImageEntity> Images { get; set; }
            = new NullCollection<ImageEntity>();

        public virtual ObservableCollection<PropertyValueEntity> CategoryPropertyValues { get; set; }
            = new NullCollection<PropertyValueEntity>();

        /// <summary>
        /// It new navigation property for link replace to stupid CategoryLink (will be removed later)
        /// </summary>
        public virtual ObservableCollection<CategoryRelationEntity> OutgoingLinks { get; set; }
            = new NullCollection<CategoryRelationEntity>();

        public virtual ObservableCollection<CategoryRelationEntity> IncomingLinks { get; set; }
            = new NullCollection<CategoryRelationEntity>();

        public virtual ObservableCollection<PropertyEntity> Properties { get; set; }
            = new NullCollection<PropertyEntity>();

        public virtual ObservableCollection<SeoInfoEntity> SeoInfos { get; set; }
            = new NullCollection<SeoInfoEntity>();

        public virtual ObservableCollection<CategoryDescriptionEntity> CategoryDescriptions { get; set; }
            = new NullCollection<CategoryDescriptionEntity>();

        public virtual IList<string> ExcludedProperties { get; set; }

        #endregion

        public virtual Category ToModel(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            category.Id = Id;
            category.CreatedBy = CreatedBy;
            category.CreatedDate = CreatedDate;
            category.ModifiedBy = ModifiedBy;
            category.ModifiedDate = ModifiedDate;
            category.OuterId = OuterId;
            category.Code = Code;
            category.Name = Name;
            category.Priority = Priority;
            category.TaxType = TaxType;
            category.CatalogId = CatalogId;
            category.ParentId = ParentCategoryId;
            category.IsActive = IsActive;
            category.EnableDescription = EnableDescription;
            category.ExcludedProperties = ExcludedProperties?.Select(x => new ExcludedProperty(x)).ToList();
            category.Links = OutgoingLinks.Select(x => x.ToModel(new CategoryLink())).ToList();
            category.Images = Images.OrderBy(x => x.SortOrder).Select(x => x.ToModel(AbstractTypeFactory<Image>.TryCreateInstance())).ToList();
            category.Descriptions = CategoryDescriptions.Select(x => x.ToModel(AbstractTypeFactory<CategoryDescription>.TryCreateInstance())).ToList();
            category.SeoInfos = SeoInfos.Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();

            category.Properties = Properties.Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance()))
                                           .OrderBy(x => x.Name)
                                           .ToList();

            if (LocalizedNames != null)
            {
                category.LocalizedName = new LocalizedString();
                foreach (var localizedName in LocalizedNames)
                {
                    category.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
                }
            }

            foreach (var property in category.Properties)
            {
                property.IsReadOnly = property.Type != PropertyType.Category;
            }

            // Transform property value into transient properties
            if (!CategoryPropertyValues.IsNullOrEmpty())
            {
                var propertyValues = CategoryPropertyValues.OrderBy(x => x.DictionaryItem?.SortOrder)
                                                       .ThenBy(x => x.Name)
                                                       .SelectMany(pv => pv.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance()).ToList());

                var transientInstanceProperties = propertyValues.GroupBy(pv => pv.PropertyName).Select(values =>
                {
                    var property = AbstractTypeFactory<Property>.TryCreateInstance();
                    property.Type = PropertyType.Category;
                    property.Name = values.Key;
                    property.ValueType = values.First().ValueType;
                    property.Values = values.ToList();
                    foreach (var propValue in property.Values)
                    {
                        propValue.Property = property;
                    }
                    return property;
                }).OrderBy(x => x.Name).ToList();

                foreach (var transientInstanceProperty in transientInstanceProperties)
                {
                    var existSelfProperty = category.Properties.FirstOrDefault(x => x.IsSame(transientInstanceProperty, PropertyType.Category));
                    if (existSelfProperty == null)
                    {
                        category.Properties.Add(transientInstanceProperty);
                    }
                    else
                    {
                        // Just only copy values for existing self property
                        existSelfProperty.Values = transientInstanceProperty.Values;
                    }
                }
            }
            return category;
        }

        public virtual CategoryEntity FromModel(Category category, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(category);

            pkMap.AddPair(category, this);

            Id = category.Id;
            CreatedBy = category.CreatedBy;
            CreatedDate = category.CreatedDate;
            ModifiedBy = category.ModifiedBy;
            ModifiedDate = category.ModifiedDate;
            OuterId = category.OuterId;

            Code = category.Code;
            Name = category.Name;
            Priority = category.Priority;
            TaxType = category.TaxType;
            CatalogId = category.CatalogId;

            ParentCategoryId = category.ParentId;
            EndDate = DateTime.UtcNow.AddYears(100);
            StartDate = DateTime.UtcNow;
            IsActive = category.IsActive ?? true;
            EnableDescription = category.EnableDescription;
            ExcludedProperties = category.ExcludedProperties?
                .Where(x => !x.IsInherited)
                .Select(x => x.Name)
                .ToList();

            if (!category.Properties.IsNullOrEmpty())
            {
                var propValues = new List<PropertyValue>();
                var forceSavePropertyValues = false;

                foreach (var property in category.Properties.Where(x => x.Type == PropertyType.Category))
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
                    CategoryPropertyValues = new ObservableCollection<PropertyValueEntity>(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModels(propValues, pkMap));
                }
            }

            if (category.Links != null)
            {
                OutgoingLinks = new ObservableCollection<CategoryRelationEntity>(category.Links.Select(x => AbstractTypeFactory<CategoryRelationEntity>.TryCreateInstance().FromModel(x)));
            }

            if (category.Images != null)
            {
                Images = new ObservableCollection<ImageEntity>(category.Images.Select(x => AbstractTypeFactory<ImageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (category.SeoInfos != null)
            {
                SeoInfos = new ObservableCollection<SeoInfoEntity>(category.SeoInfos.Select(x => AbstractTypeFactory<SeoInfoEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (category.Descriptions != null)
            {
                CategoryDescriptions = new ObservableCollection<CategoryDescriptionEntity>(category.Descriptions.Where(x => !x.IsInherited).Select(x => AbstractTypeFactory<CategoryDescriptionEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (category.LocalizedName != null)
            {
                LocalizedNames = new ObservableCollection<CategoryLocalizedNameEntity>(category.LocalizedName.Values
                    .Select(x =>
                    {
                        var entity = AbstractTypeFactory<CategoryLocalizedNameEntity>.TryCreateInstance();
                        entity.LanguageCode = x.Key;
                        entity.Value = x.Value;
                        return entity;
                    }));
            }

            return this;
        }

        public virtual void Patch(CategoryEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.CatalogId = CatalogId;
            target.ParentCategoryId = ParentCategoryId;
            target.Code = Code;
            target.Name = Name;
            target.TaxType = TaxType;
            target.Priority = Priority;
            target.IsActive = IsActive;
            target.EnableDescription = EnableDescription;
            target.ExcludedProperties = ExcludedProperties;

            if (!CategoryPropertyValues.IsNullCollection())
            {
                CategoryPropertyValues.Patch(target.CategoryPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }

            if (!OutgoingLinks.IsNullCollection())
            {
                var categoryRelationComparer = AnonymousComparer.Create((CategoryRelationEntity x) => string.Join(":", x.TargetCatalogId, x.TargetCategoryId));
                OutgoingLinks.Patch(target.OutgoingLinks, categoryRelationComparer, (sourceLink, targetLink) => sourceLink.Patch(targetLink));
            }

            if (!Images.IsNullCollection())
            {
                Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }

            if (!SeoInfos.IsNullCollection())
            {
                SeoInfos.Patch(target.SeoInfos, (sourceSeoInfo, targetSeoInfo) => sourceSeoInfo.Patch(targetSeoInfo));
            }

            if (!CategoryDescriptions.IsNullCollection())
            {
                CategoryDescriptions.Patch(target.CategoryDescriptions, (sourceDescription, targetDescription) => sourceDescription.Patch(targetDescription));
            }

            if (!LocalizedNames.IsNullCollection())
            {
                var localizedNameComparer = AnonymousComparer.Create((CategoryLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
                LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (sourceValue, targetValue) => { });
            }
        }
    }
}
