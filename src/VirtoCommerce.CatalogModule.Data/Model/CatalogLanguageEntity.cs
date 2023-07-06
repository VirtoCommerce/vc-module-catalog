using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CatalogLanguageEntity : Entity
    {
        [StringLength(64)]
        public string Language { get; set; }

        public int Priority { get; set; }

        #region Navigation Properties

        public string CatalogId { get; set; }
        public virtual CatalogEntity Catalog { get; set; }

        #endregion

        public virtual CatalogLanguage ToModel(CatalogLanguage lang)
        {
            if (lang == null)
            {
                throw new ArgumentNullException(nameof(lang));
            }

            lang.Id = Id;
            lang.CatalogId = Id;
            lang.LanguageCode = Language;
            lang.Priority = Priority;

            return lang;
        }

        public virtual CatalogLanguageEntity FromModel(CatalogLanguage lang, PrimaryKeyResolvingMap pkMap)
        {
            if (lang == null)
            {
                throw new ArgumentNullException(nameof(lang));
            }

            pkMap.AddPair(lang, this);

            Id = lang.Id;
            CatalogId = lang.Id;
            Language = lang.LanguageCode;
            Priority = lang.Priority;

            return this;
        }

        public virtual void Patch(CatalogLanguageEntity target)
        {
            target.Language = Language;
            target.Priority = Priority;
        }
    }
}
