using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class AssetEntity : AuditableEntity, IHasOuterId
    {
        [StringLength(2083)]
        [Required]
        public string Url { get; set; }

        [StringLength(1024)]
        public string Name { get; set; }

        [StringLength(128)]
        public string MimeType { get; set; }

        public long Size { get; set; }

        [StringLength(5)]
        public string LanguageCode { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        public int SortOrder { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [StringLength(64)]
        public string Group { get; set; }

        #region Navigation Properties

        public string ItemId { get; set; }
        public virtual ItemEntity CatalogItem { get; set; }

        #endregion

        public virtual Asset ToModel(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            asset.Id = Id;
            asset.CreatedBy = CreatedBy;
            asset.CreatedDate = CreatedDate;
            asset.ModifiedBy = ModifiedBy;
            asset.ModifiedDate = ModifiedDate;
            asset.OuterId = OuterId;

            asset.LanguageCode = LanguageCode;
            asset.Name = Name;
            asset.MimeType = MimeType;
            asset.Url = Url;
            asset.RelativeUrl = Url;
            asset.Size = Size;
            asset.SortOrder = SortOrder;
            asset.Description = Description;
            asset.Group = Group;

            if (!(string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(LanguageCode) && string.IsNullOrEmpty(Url)))
            {
                var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                seoInfo.Name = Name;
                seoInfo.MetaDescription = Description;
                seoInfo.LanguageCode = LanguageCode;
                seoInfo.SemanticUrl = Url;
                asset.SeoInfos = new List<SeoInfo> {seoInfo};
            }

            return asset;
        }

        public virtual AssetEntity FromModel(Asset asset, PrimaryKeyResolvingMap pkMap)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            pkMap.AddPair(asset, this);

            Id = asset.Id;
            CreatedBy = asset.CreatedBy;
            CreatedDate = asset.CreatedDate;
            ModifiedBy = asset.ModifiedBy;
            ModifiedDate = asset.ModifiedDate;
            OuterId = asset.OuterId;

            LanguageCode = asset.LanguageCode;
            Name = asset.Name;
            MimeType = asset.MimeType;
            Url = !string.IsNullOrEmpty(asset.RelativeUrl) ? asset.RelativeUrl : asset.Url;
            Size = asset.Size;
            Group = asset.Group;
            SortOrder = asset.SortOrder;
            Description = asset.Description;

            return this;
        }

        public virtual void Patch(AssetEntity target)
        {
            target.LanguageCode = LanguageCode;
            target.Name = Name;
            target.MimeType = MimeType;
            target.Url = Url;
            target.Size = Size;
            target.Description = Description;
            target.Group = Group;
            target.SortOrder = SortOrder;
        }
    }
}
