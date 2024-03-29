
using System;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class CatalogLanguage : Entity, IHasLanguageCode, ICloneable, IHasCatalogId
    {
        public string CatalogId { get; set; }
        [JsonIgnore]
        public Catalog Catalog { get; set; }

        public bool IsDefault { get; set; }

        /// <summary>
        /// Lowest number wins
        /// </summary>
        public int Priority { get; set; }
        #region IHasLanguageCode members
        public string LanguageCode { get; set; }

        #endregion
        #region ICloneable members
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
