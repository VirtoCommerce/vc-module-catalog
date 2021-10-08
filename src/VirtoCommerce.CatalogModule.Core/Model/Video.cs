using System;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    /// <summary>
    /// Video content information
    /// </summary>
    /// <remarks>
    /// Require adding VideoType property
    /// </remarks>
    public class Video : AuditableEntity, IHasName, IHasLanguage, ICloneable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public DateTime? UploadDate { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ContentUrl { get; set; }
        public string EmbedUrl { get; set; }
        public string Duration { get; set; }

        #region IHasLanguage members
        public string LanguageCode { get; set; }
        #endregion

        public string OwnerId { get; set; }
        public string OwnerType { get; set; }

        #region ICloneable members
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
