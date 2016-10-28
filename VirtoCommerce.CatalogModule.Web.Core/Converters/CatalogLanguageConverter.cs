using Omu.ValueInjecter;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class CatalogLanguageConverter
    {
        public static webModel.CatalogLanguage ToWebModel(this moduleModel.CatalogLanguage language)
        {
            var result = new webModel.CatalogLanguage();
            //Do not use omu.InjectFrom for performance reasons 
            result.CatalogId = language.CatalogId;
            result.LanguageCode = language.LanguageCode;
            result.IsDefault = language.IsDefault;
            
            return result;
        }


        public static moduleModel.CatalogLanguage ToCoreModel(this webModel.CatalogLanguage language)
        {
            var result = new moduleModel.CatalogLanguage();
            result.InjectFrom(language);
            return result;
        }
    }
}
