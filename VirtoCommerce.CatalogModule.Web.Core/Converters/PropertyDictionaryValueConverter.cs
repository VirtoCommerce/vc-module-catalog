using Omu.ValueInjecter;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class PropertyDictionaryValue
    {
        public static webModel.PropertyDictionaryValue ToWebModel(this moduleModel.PropertyDictionaryValue propDictValue)
        {
            var retVal = new webModel.PropertyDictionaryValue();
            retVal.Id = propDictValue.Id;
            retVal.PropertyId = propDictValue.PropertyId;
            retVal.Value = propDictValue.Value;
            retVal.LanguageCode = propDictValue.LanguageCode;
            retVal.Alias = propDictValue.Alias;           

            return retVal;
        }

        public static moduleModel.PropertyDictionaryValue ToCoreModel(this webModel.PropertyDictionaryValue propDictValue)
        {
            var retVal = new moduleModel.PropertyDictionaryValue();
            retVal.InjectFrom(propDictValue);
            return retVal;
        }
    }
}
