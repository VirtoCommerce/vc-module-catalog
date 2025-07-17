using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class PropertyDictionaryItemLocalizedValue : ValueObject, IHasLanguageCode
    {
        #region ILanguageSupport members
        public string LanguageCode { get; set; }
        #endregion
        public string Value { get; set; }
    }
}
