using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    /// <summary>
    /// Base class for assets.
    /// </summary>
    public abstract class AssetBase : AuditableEntity, IHasLanguage, IInheritable, ICloneable, ISeoSupport, IHasOuterId, ICopyable
    {
        protected AssetBase(string typeName)
        {
            TypeId = typeName;
        }
        public string RelativeUrl { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        [JsonIgnore]
        public bool HasExternalUrl
        {
            get
            {
                return string.IsNullOrEmpty(RelativeUrl) || Uri.IsWellFormedUriString(RelativeUrl, UriKind.Absolute);
            }
        }

        /// <summary>
        /// Gets or sets the asset type identifier.
        /// </summary>
        /// <value>
        /// The type identifier.
        /// </value>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets or sets the asset group name.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the asset name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OuterId { get; set; }

        #region IHasLanguage members
        /// <summary>
        /// Gets or sets the asset language.
        /// </summary>
        /// <value>
        /// The language code.
        /// </value>
        public string LanguageCode { get; set; }
        #endregion

        #region IInheritable members
        /// <summary>
        /// System flag used to mark that object was inherited from other
        /// </summary>
        public bool IsInherited { get; private set; }
        public virtual void TryInheritFrom(IEntity parent)
        {
            if (parent is AssetBase parentAssetBase)
            {
                Id = null;
                IsInherited = true;
                LanguageCode = parentAssetBase.LanguageCode;
                Name = parentAssetBase.Name;
                Group = parentAssetBase.Group;
                TypeId = parentAssetBase.TypeId;
                Url = parentAssetBase.Url;
                RelativeUrl = !string.IsNullOrEmpty(parentAssetBase.RelativeUrl) ? parentAssetBase.RelativeUrl : parentAssetBase.Url;
            }
        }
        #endregion


        #region ISeoSupport members
        public string SeoObjectType { get { return GetType().Name; } }

        public IList<SeoInfo> SeoInfos { get; set; }

        #endregion

        #region ICopyable members
        public virtual object GetCopy()
        {
            var result = Clone() as AssetBase;
            result.Id = null;
            return result;
        }
        #endregion

        #region ICloneable members
        public virtual object Clone()
        {
            var result = MemberwiseClone() as AssetBase;
            if (SeoInfos != null)
            {
                result.SeoInfos = SeoInfos.Select(x => x.Clone()).OfType<SeoInfo>().ToList();
            }
            return result;
        }
        #endregion

        #region Conditional JSON serialization for properties declared in base type
        public override bool ShouldSerializeAuditableProperties => false;
        #endregion
    }
}
