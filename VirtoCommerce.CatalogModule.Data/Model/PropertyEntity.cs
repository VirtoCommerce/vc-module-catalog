using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyEntity : AuditableEntity
    {
        public PropertyEntity()
        {
            DictionaryValues = new NullCollection<PropertyDictionaryValueEntity>();
            PropertyAttributes = new NullCollection<PropertyAttributeEntity>();
            DisplayNames = new NullCollection<PropertyDisplayNameEntity>();
            ValidationRules = new NullCollection<PropertyValidationRuleEntity>();
        }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(128)]
        public string TargetType { get; set; }

        public bool IsKey { get; set; }

        public bool IsSale { get; set; }

        public bool IsEnum { get; set; }

        public bool IsInput { get; set; }

        public bool IsRequired { get; set; }

        public bool IsMultiValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is locale dependant. If true, the locale must be specified for the values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is locale dependant; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocaleDependant { get; set; }

        public bool AllowAlias { get; set; }

        [Required]
        public int PropertyValueType { get; set; }

        #region Navigation Properties

        public string CatalogId { get; set; }
        public virtual CatalogEntity Catalog { get; set; }

        public string CategoryId { get; set; }
        public virtual CategoryEntity Category { get; set; }

        public virtual ObservableCollection<PropertyDictionaryValueEntity> DictionaryValues { get; set; }
        public virtual ObservableCollection<PropertyAttributeEntity> PropertyAttributes { get; set; }
        public virtual ObservableCollection<PropertyDisplayNameEntity> DisplayNames { get; set; }
        public virtual ObservableCollection<PropertyValidationRuleEntity> ValidationRules { get; set; }

        #endregion

        public virtual Property ToModel(Property property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            property.Id = this.Id;
            property.CreatedBy = this.CreatedBy;
            property.CreatedDate = this.CreatedDate;
            property.ModifiedBy = this.ModifiedBy;
            property.ModifiedDate = this.ModifiedDate;

            property.CatalogId = this.CatalogId;
            property.CategoryId = this.CategoryId;


            property.Name = this.Name;
            property.Required = this.IsRequired;
            property.Multivalue = this.IsMultiValue;
            property.Multilanguage = this.IsLocaleDependant;
            property.Dictionary = this.IsEnum;
            property.ValueType = (PropertyValueType)this.PropertyValueType;
            property.Type = EnumUtility.SafeParse(this.TargetType, PropertyType.Catalog);

            property.DictionaryValues = this.DictionaryValues.Select(x => x.ToModel(AbstractTypeFactory<PropertyDictionaryValue>.TryCreateInstance())).ToList();
            property.Attributes = this.PropertyAttributes.Select(x => x.ToModel(AbstractTypeFactory<PropertyAttribute>.TryCreateInstance())).ToList();
            property.DisplayNames = this.DisplayNames.Select(x => x.ToModel(AbstractTypeFactory<PropertyDisplayName>.TryCreateInstance())).ToList();
            property.ValidationRules = this.ValidationRules.Select(x => x.ToModel(AbstractTypeFactory<PropertyValidationRule>.TryCreateInstance())).ToList();
            foreach (var rule in property.ValidationRules)
            {
                rule.Property = property;
            }

            return property;
        }

        public virtual PropertyEntity FromModel(Property property, PrimaryKeyResolvingMap pkMap)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            pkMap.AddPair(property, this);

            this.Id = property.Id;
            this.CreatedBy = property.CreatedBy;
            this.CreatedDate = property.CreatedDate;
            this.ModifiedBy = property.ModifiedBy;
            this.ModifiedDate = property.ModifiedDate;

            this.CatalogId = property.CatalogId;
            this.CategoryId = property.CategoryId;
            
            this.Name = property.Name;
            this.PropertyValueType = (int)property.ValueType;
            this.IsMultiValue = property.Multivalue;
            this.IsLocaleDependant = property.Multilanguage;
            this.IsEnum = property.Dictionary;
            this.IsRequired = property.Required;
            this.TargetType = property.Type.ToString();
  
            if (property.Attributes != null)
            {
                this.PropertyAttributes = new ObservableCollection<PropertyAttributeEntity>(property.Attributes.Select(x => AbstractTypeFactory<PropertyAttributeEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (property.DictionaryValues != null)
            {
                this.DictionaryValues = new ObservableCollection<PropertyDictionaryValueEntity>(property.DictionaryValues.Select(x => AbstractTypeFactory<PropertyDictionaryValueEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (property.DisplayNames != null)
            {
                this.DisplayNames = new ObservableCollection<PropertyDisplayNameEntity>(property.DisplayNames.Select(x => AbstractTypeFactory<PropertyDisplayNameEntity>.TryCreateInstance().FromModel(x)));
            }

            if (property.ValidationRules != null)
            {
                this.ValidationRules = new ObservableCollection<PropertyValidationRuleEntity>(property.ValidationRules.Select(x => AbstractTypeFactory<PropertyValidationRuleEntity>.TryCreateInstance().FromModel(x)));
            }
            return this;
        }

        public virtual void Patch(PropertyEntity target)
        {
            target.PropertyValueType = this.PropertyValueType;
            target.IsEnum = this.IsEnum;
            target.IsMultiValue = this.IsMultiValue;
            target.IsLocaleDependant = this.IsLocaleDependant;
            target.IsRequired = this.IsRequired;
            target.TargetType = this.TargetType;
            target.Name = this.Name;

            if (!this.PropertyAttributes.IsNullCollection())
            {
                var attributeComparer = AnonymousComparer.Create((PropertyAttributeEntity x) => x.IsTransient() ? x.PropertyAttributeName : x.Id);
                this.PropertyAttributes.Patch(target.PropertyAttributes, attributeComparer, (sourceAsset, targetAsset) => sourceAsset.Patch(targetAsset));
            }
            if (!this.DictionaryValues.IsNullCollection())
            {
                this.DictionaryValues.Patch(target.DictionaryValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }
            if (!this.DisplayNames.IsNullCollection())
            {
                var displayNamesComparer = AnonymousComparer.Create((PropertyDisplayNameEntity x) => $"{x.Name}-{x.Locale}");
                this.DisplayNames.Patch(target.DisplayNames, displayNamesComparer, (sourceDisplayName, targetDisplayName) => sourceDisplayName.Patch(targetDisplayName));
            }

            if (!this.ValidationRules.IsNullCollection())
            {
                this.ValidationRules.Patch(target.ValidationRules, (sourceRule, targetRule) => sourceRule.Patch(targetRule));
            }
        }
    }
}
