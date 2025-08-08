using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyDictionaryItemEntity : Entity, IDataEntity<PropertyDictionaryItemEntity, PropertyDictionaryItem>
    {
        [StringLength(512)]
        [Required]
        public string Alias { get; set; }

        [StringLength(32)]
        public string ColorCode { get; set; }

        public int SortOrder { get; set; }

        #region Navigation Properties

        public string PropertyId { get; set; }
        public virtual PropertyEntity Property { get; set; }

        public ObservableCollection<PropertyDictionaryValueEntity> DictionaryItemValues { get; set; }
            = new NullCollection<PropertyDictionaryValueEntity>();

        #endregion

        public virtual PropertyDictionaryItem ToModel(PropertyDictionaryItem propDictItem)
        {
            if (propDictItem == null)
            {
                throw new ArgumentNullException(nameof(propDictItem));
            }
            propDictItem.Id = Id;
            propDictItem.Alias = Alias;
            propDictItem.ColorCode = ColorCode;
            propDictItem.SortOrder = SortOrder;
            propDictItem.PropertyId = PropertyId;
            propDictItem.LocalizedValues = DictionaryItemValues.Select(x => x.ToModel(AbstractTypeFactory<PropertyDictionaryItemLocalizedValue>.TryCreateInstance())).ToList();

            return propDictItem;
        }

        public virtual PropertyDictionaryItemEntity FromModel(PropertyDictionaryItem propDictItem, PrimaryKeyResolvingMap pkMap)
        {
            if (propDictItem == null)
            {
                throw new ArgumentNullException(nameof(propDictItem));
            }
            pkMap.AddPair(propDictItem, this);

            Id = propDictItem.Id;
            Alias = propDictItem.Alias;
            ColorCode = propDictItem.ColorCode;
            SortOrder = propDictItem.SortOrder;
            PropertyId = propDictItem.PropertyId;
            if (propDictItem.LocalizedValues != null)
            {
                DictionaryItemValues = new ObservableCollection<PropertyDictionaryValueEntity>(propDictItem.LocalizedValues.Select(x => AbstractTypeFactory<PropertyDictionaryValueEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(PropertyDictionaryItemEntity target)
        {
            target.Alias = Alias;
            target.ColorCode = ColorCode;
            target.SortOrder = SortOrder;
            if (!DictionaryItemValues.IsNullCollection())
            {
                var comparer = AnonymousComparer.Create((PropertyDictionaryValueEntity x) => x.Value + '|' + x.Locale);
                DictionaryItemValues.Patch(target.DictionaryItemValues, comparer, (sourceDictItem, targetDictItem) => sourceDictItem.Patch(targetDictItem));
            }
        }
    }
}
