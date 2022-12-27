using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyValueEntity2 : PropertyValueEntity
    {
        public override void Patch(PropertyValueEntity target)
        {
            base.Patch(target);
        }
        public override PropertyValueEntity FromModel(PropertyValue propValue, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(propValue, pkMap);
        }
        public override IEnumerable<PropertyValue> ToModel(PropertyValue propValue)
        {
            return base.ToModel(propValue);
        }
        public override IEnumerable<PropertyValueEntity> FromModels(IEnumerable<PropertyValue> propValues, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModels(propValues, pkMap);
        }
        protected override object GetValue(PropertyValueType valueType)
        {
            return base.GetValue(valueType);
        }
        protected override void SetValue(PropertyValueType valueType, object value)
        {
            base.SetValue(valueType, value);
        }
    }
}
