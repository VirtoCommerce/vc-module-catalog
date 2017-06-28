using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyDictionaryValueEntity : Entity
    {
        [StringLength(64)]
        public string Alias { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Value { get; set; }

        [StringLength(64)]
        public string Locale { get; set; }

        #region Navigation Properties
        public string PropertyId { get; set; }
        public virtual PropertyEntity Property { get; set; }
        #endregion

        public virtual PropertyDictionaryValue ToModel(PropertyDictionaryValue dictValue)
        {
            if (dictValue == null)
                throw new ArgumentNullException(nameof(dictValue));

            dictValue.Id = this.Id;
            dictValue.Alias = this.Alias;
            dictValue.LanguageCode = this.Locale;
            dictValue.PropertyId = this.PropertyId;
            dictValue.Value = this.Value;

            return dictValue;
        }

        public virtual PropertyDictionaryValueEntity FromModel(PropertyDictionaryValue dictValue, PrimaryKeyResolvingMap pkMap)
        {
            if (dictValue == null)
                throw new ArgumentNullException(nameof(dictValue));

            pkMap.AddPair(dictValue, this);

            this.Id = dictValue.Id;
            this.Alias = dictValue.Alias;
            this.Value = dictValue.Value;
            this.PropertyId = dictValue.PropertyId;
            this.Locale = dictValue.LanguageCode;

            return this;
        }

        public virtual void Patch(PropertyDictionaryValueEntity target)
        {
            target.Value = this.Value;
            target.Alias = this.Alias;
            target.Locale = this.Locale;
        }
    }
}
