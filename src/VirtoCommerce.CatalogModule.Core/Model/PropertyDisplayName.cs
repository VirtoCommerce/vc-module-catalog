using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class PropertyDisplayName : ValueObject, IHasLanguageCode
    {
        public string Name { get; set; }
        #region IHasLanguage members
        public string LanguageCode { get; set; }
        #endregion
    }
}
