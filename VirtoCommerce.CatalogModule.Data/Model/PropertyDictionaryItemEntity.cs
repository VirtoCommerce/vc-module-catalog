using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyDictionaryItemEntity : Entity
    {
        public PropertyDictionaryItemEntity()
        {
            DictionaryItemValues = new NullCollection<PropertyDictionaryValueEntity>();
        }

        [StringLength(512)]
        public string Alias { get; set; }

        #region Navigation Properties
        public string PropertyId { get; set; }
        public virtual PropertyEntity Property { get; set; }

        public virtual ObservableCollection<PropertyDictionaryValueEntity> DictionaryItemValues { get; set; }
        #endregion

        public virtual void Patch(PropertyDictionaryItemEntity target)
        {
            if (!DictionaryItemValues.IsNullCollection())
            {
                var comparer = AnonymousComparer.Create((PropertyDictionaryValueEntity x) => x.IsTransient() ? x.Value + '|' + x.Locale : x.Id);
                DictionaryItemValues.Patch(target.DictionaryItemValues, comparer, (sourceDictItem, targetDictItem) => sourceDictItem.Patch(targetDictItem));
            }
        }

    }
}
