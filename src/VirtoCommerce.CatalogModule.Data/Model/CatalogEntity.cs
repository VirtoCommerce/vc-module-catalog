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
    public class CatalogEntity : AuditableEntity, IHasOuterId, IDataEntity<CatalogEntity, Catalog>
    {
        public bool Virtual { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(64)]
        [Required]
        public string DefaultLanguage { get; set; }

        [StringLength(128)]
        public string OwnerId { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<CategoryRelationEntity> IncomingLinks { get; set; }
            = new NullCollection<CategoryRelationEntity>();

        public virtual ObservableCollection<CatalogLanguageEntity> CatalogLanguages { get; set; }
            = new NullCollection<CatalogLanguageEntity>();

        public virtual ObservableCollection<PropertyValueEntity> CatalogPropertyValues { get; set; }
            = new NullCollection<PropertyValueEntity>();

        public virtual ObservableCollection<PropertyEntity> Properties { get; set; }
            = new NullCollection<PropertyEntity>();

        public virtual ObservableCollection<PropertyGroupEntity> PropertyGroups { get; set; }
            = new NullCollection<PropertyGroupEntity>();

        public virtual ObservableCollection<SeoInfoEntity> SeoInfos { get; set; }
            = new NullCollection<SeoInfoEntity>();

        #endregion

        public virtual Catalog ToModel(Catalog catalog)
        {
            ArgumentNullException.ThrowIfNull(catalog);

            catalog.Id = Id;

            catalog.CreatedDate = CreatedDate;
            catalog.ModifiedDate = ModifiedDate;
            catalog.CreatedBy = CreatedBy;
            catalog.ModifiedBy = ModifiedBy;

            catalog.Name = Name;
            catalog.IsVirtual = Virtual;
            catalog.OuterId = OuterId;

            catalog.Languages = new List<CatalogLanguage>();
            var defaultLanguage = (new CatalogLanguageEntity { Language = string.IsNullOrEmpty(DefaultLanguage) ? "en-us" : DefaultLanguage }).ToModel(AbstractTypeFactory<CatalogLanguage>.TryCreateInstance());
            defaultLanguage.IsDefault = true;
            catalog.Languages.Add(defaultLanguage);
            // Populate additional languages
            foreach (var additionalLanguage in CatalogLanguages.Where(x => x.Language != defaultLanguage.LanguageCode).Select(x => x.ToModel(AbstractTypeFactory<CatalogLanguage>.TryCreateInstance())))
            {
                catalog.Languages.Add(additionalLanguage);
            }

            catalog.SeoInfos = SeoInfos.Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();

            catalog.PropertyGroups = PropertyGroups.Select(x => x.ToModel(AbstractTypeFactory<PropertyGroup>.TryCreateInstance())).ToList();

            // Self properties
            catalog.Properties = Properties.Where(x => x.CategoryId == null)
                .OrderBy(x => x.Name)
                .Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToList();


            foreach (var property in catalog.Properties)
            {
                property.IsReadOnly = property.Type != PropertyType.Catalog;
                property.Values = CatalogPropertyValues.Where(pr => pr.Name.EqualsIgnoreCase(property.Name)).OrderBy(x => x.DictionaryItem?.SortOrder)
                    .ThenBy(x => x.Name)
                    .SelectMany(x => x.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance())).ToList();
            }

            return catalog;
        }

        public virtual CatalogEntity FromModel(Catalog catalog, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(catalog);

            if (catalog.DefaultLanguage == null)
            {
                throw new NullReferenceException("DefaultLanguage");
            }

            pkMap.AddPair(catalog, this);

            Id = catalog.Id;

            CreatedDate = catalog.CreatedDate;
            ModifiedDate = catalog.ModifiedDate;
            CreatedBy = catalog.CreatedBy;
            ModifiedBy = catalog.ModifiedBy;

            Name = catalog.Name;
            Virtual = catalog.IsVirtual;
            OuterId = catalog.OuterId;

            DefaultLanguage = catalog.DefaultLanguage.LanguageCode;

            if (!catalog.Properties.IsNullOrEmpty())
            {
                var propValues = new List<PropertyValue>();
                var forceSavePropertyValues = false;

                foreach (var property in catalog.Properties)
                {
                    if (property.Values != null)
                    {
                        var propertyValues = property.Values.Where(pv => pv != null).ToList();

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
                    CatalogPropertyValues = new ObservableCollection<PropertyValueEntity>(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModels(propValues, pkMap));
                }
            }

            if (catalog.PropertyGroups != null)
            {
                PropertyGroups = new ObservableCollection<PropertyGroupEntity>(catalog.PropertyGroups.Select(x => AbstractTypeFactory<PropertyGroupEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (catalog.SeoInfos != null)
            {
                SeoInfos = new ObservableCollection<SeoInfoEntity>(catalog.SeoInfos.Select(x => AbstractTypeFactory<SeoInfoEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (catalog.Languages != null)
            {
                CatalogLanguages = new ObservableCollection<CatalogLanguageEntity>(catalog.Languages.Select(x => AbstractTypeFactory<CatalogLanguageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(CatalogEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.Name = Name;
            target.DefaultLanguage = DefaultLanguage;

            // Languages patch
            if (!CatalogLanguages.IsNullCollection())
            {
                var languageComparer = AnonymousComparer.Create((CatalogLanguageEntity x) => x.Language);
                CatalogLanguages.Patch(target.CatalogLanguages, languageComparer,
                                                     (sourceLang, targetlang) => sourceLang.Patch(targetlang));
            }

            // Property values
            if (!CatalogPropertyValues.IsNullCollection())
            {
                CatalogPropertyValues.Patch(target.CatalogPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }

            if (!PropertyGroups.IsNullCollection())
            {
                PropertyGroups.Patch(target.PropertyGroups, (sourceSeoInfo, targetSeoInfo) => sourceSeoInfo.Patch(targetSeoInfo));
            }

            if (!SeoInfos.IsNullCollection())
            {
                SeoInfos.Patch(target.SeoInfos, (sourceSeoInfo, targetSeoInfo) => sourceSeoInfo.Patch(targetSeoInfo));
            }
        }
    }
}
