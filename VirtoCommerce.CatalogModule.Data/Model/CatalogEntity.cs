using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CatalogEntity : AuditableEntity
    {
        public CatalogEntity()
        {

            CatalogLanguages = new NullCollection<CatalogLanguageEntity>();
            CatalogPropertyValues = new NullCollection<PropertyValueEntity>();
            IncommingLinks = new NullCollection<CategoryRelationEntity>();
            Properties = new NullCollection<PropertyEntity>();
        }

        public bool Virtual { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(64)]
        [Required]
        public string DefaultLanguage { get; set; }

        [StringLength(128)]
        public string OwnerId { get; set; }

        #region Navigation Properties
        public virtual ObservableCollection<CategoryRelationEntity> IncommingLinks { get; set; }

        public virtual ObservableCollection<CatalogLanguageEntity> CatalogLanguages { get; set; }
        public virtual ObservableCollection<PropertyValueEntity> CatalogPropertyValues { get; set; }
        public virtual ObservableCollection<PropertyEntity> Properties { get; set; }
        #endregion


        public virtual Catalog ToModel(Catalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            catalog.Id = this.Id;         
            catalog.Name = this.Name;
            catalog.IsVirtual = this.Virtual;

            catalog.Languages = new List<CatalogLanguage>();
            var defaultLanguage = (new CatalogLanguageEntity { Language = string.IsNullOrEmpty(this.DefaultLanguage) ? "en-us" : this.DefaultLanguage }).ToModel(AbstractTypeFactory<CatalogLanguage>.TryCreateInstance());
            defaultLanguage.IsDefault = true;
            catalog.Languages.Add(defaultLanguage);
            //populate additional languages
            foreach (var additionalLanguage in this.CatalogLanguages.Where(x => x.Language != defaultLanguage.LanguageCode).Select(x => x.ToModel(AbstractTypeFactory<CatalogLanguage>.TryCreateInstance())))
            {
                catalog.Languages.Add(additionalLanguage);
            }
            catalog.PropertyValues = this.CatalogPropertyValues.Select(x => x.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance())).ToList();
            //Self properties
            catalog.Properties = this.Properties.Where(x => x.CategoryId == null)
                                                .OrderBy(x => x.Name)
                                                .Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToList();
            
            return catalog;
        }

        public virtual CatalogEntity FromModel(Catalog catalog, PrimaryKeyResolvingMap pkMap)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            if (catalog.DefaultLanguage == null)
                throw new NullReferenceException("DefaultLanguage");

            pkMap.AddPair(catalog, this);

            this.Id = catalog.Id;
            this.Name = catalog.Name;
            this.Virtual = catalog.IsVirtual;
            this.DefaultLanguage = catalog.DefaultLanguage.LanguageCode;

            if (catalog.PropertyValues != null)
            {
                this.CatalogPropertyValues = new ObservableCollection<PropertyValueEntity>();
                foreach (var propertyValue in catalog.PropertyValues)
                {
                    if (!propertyValue.IsInherited && propertyValue.Value != null && !string.IsNullOrEmpty(propertyValue.Value.ToString()))
                    {
                       this.CatalogPropertyValues.Add(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModel(propertyValue, pkMap));
                    }
                }
            }
            if (catalog.Languages != null)
            {
                this.CatalogLanguages = new ObservableCollection<CatalogLanguageEntity>(catalog.Languages.Select(x => AbstractTypeFactory<CatalogLanguageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(CatalogEntity target)
        {

            target.Name = this.Name;
            target.DefaultLanguage = this.DefaultLanguage;

            //Languages patch
            if (!this.CatalogLanguages.IsNullCollection())
            {
                var languageComparer = AnonymousComparer.Create((CatalogLanguageEntity x) => x.Language);
                this.CatalogLanguages.Patch(target.CatalogLanguages, languageComparer,
                                                     (sourceLang, targetlang) => sourceLang.Patch(targetlang));
            }

            //Property values
            if (!this.CatalogPropertyValues.IsNullCollection())
            {
                this.CatalogPropertyValues.Patch(target.CatalogPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }

        }

    }
}
