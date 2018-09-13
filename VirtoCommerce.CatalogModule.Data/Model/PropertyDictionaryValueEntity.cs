using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyDictionaryValueEntity : Entity
    {
        [StringLength(512)]
        public string Value { get; set; }

        [StringLength(64)]
        public string Locale { get; set; }

        #region Navigation Properties
        public string DictionaryItemId { get; set; }
        public virtual PropertyDictionaryItemEntity DictionaryItem { get; set; }
        #endregion

        public virtual PropertyDictionaryValue ToModel(PropertyDictionaryValue propDictValue)
        {
            if (propDictValue == null)
            {
                throw new ArgumentNullException(nameof(propDictValue));
            }
            propDictValue.Id = Id;
            //Use alias property for store DictionaryItem identifier
            propDictValue.Alias = DictionaryItem?.Id;
            propDictValue.LanguageCode = Locale;
            propDictValue.PropertyId = DictionaryItem?.PropertyId;
            propDictValue.Value = Value;
            return propDictValue;
        }

        public virtual void Patch(PropertyDictionaryValueEntity target)
        {
            target.Locale = Locale;
            target.Value = Value;
        }
    }
}
