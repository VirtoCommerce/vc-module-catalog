using System;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class CategoryDescription : AuditableEntity, IHasLanguage, ICloneable, IInheritable, ICopyable
    {
        public string Content { get; set; }
        public string DescriptionType { get; set; }

        #region ILanguageSupport Members
        public string LanguageCode { get; set; }
        #endregion

        #region IInheritable Members
        public bool IsInherited { get; set; }
        public virtual void TryInheritFrom(IEntity parent)
        {
            if (parent is CategoryDescription parentBase)
            {
                Id = null;
                IsInherited = true;
                LanguageCode = parentBase.LanguageCode;
                Content = parentBase.Content;
                DescriptionType = parentBase.DescriptionType;
            }
        }
        #endregion

        #region ICloneable members
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        #region ICopyable
        public virtual object GetCopy()
        {
            var result = Clone() as CategoryDescription;
            result.Id = null;
            return result;
        }
        #endregion

        #region Conditional JSON serialization for properties declared in base type
        public override bool ShouldSerializeAuditableProperties => false;
        #endregion
    }
}
