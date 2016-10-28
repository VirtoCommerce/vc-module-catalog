using System;
using Omu.ValueInjecter;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class PropertyValueConverter
    {
        public static webModel.PropertyValue ToWebModel(this moduleModel.PropertyValue propValue)
        {
            var retVal = new webModel.PropertyValue();

            retVal.Id = propValue.Id;
            retVal.LanguageCode = propValue.LanguageCode;
            retVal.PropertyId = propValue.PropertyId;
            retVal.PropertyName = propValue.PropertyName;
            retVal.ValueId = propValue.ValueId;
            retVal.ValueType = propValue.ValueType;

            if (propValue.Property != null)
            {
                retVal.PropertyId = propValue.Property.Id;
            }
            if(propValue.ValueType == Domain.Catalog.Model.PropertyValueType.DateTime && propValue.Value != null)
            {
                retVal.Value = DateTime.SpecifyKind(((DateTime)propValue.Value), DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                retVal.Value = (propValue.Value ?? string.Empty).ToString();
            }           

            return retVal;
        }

        public static moduleModel.PropertyValue ToCoreModel(this webModel.PropertyValue propValue)
        {
            var retVal = new moduleModel.PropertyValue();
            retVal.InjectFrom(propValue);
            retVal.Value = propValue.Value;
            retVal.ValueType = (moduleModel.PropertyValueType)(int)propValue.ValueType;
            return retVal;
        }
    }
}
