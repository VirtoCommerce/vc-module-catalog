using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class VideoEntity : AuditableEntity, IDataEntity<VideoEntity, Video>
    {
        [Required, StringLength(1024)]
        public string Name { get; set; }

        [Required, StringLength(1024)]
        public string Description { get; set; }

        public int SortOrder { get; set; }

        public DateTime UploadDate { get; set; }

        [Required, StringLength(2083)]
        public string ThumbnailUrl { get; set; }

        [Required, StringLength(2083)]
        public string ContentUrl { get; set; }

        [StringLength(2083)]
        public string EmbedUrl { get; set; }

        [StringLength(20)]
        public string Duration { get; set; }

        [StringLength(64)]
        public string LanguageCode { get; set; }

        [Required, StringLength(128)]
        public string OwnerId { get; set; }

        [Required, StringLength(256)]
        public string OwnerType { get; set; }

        public virtual Video ToModel(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            video.Id = Id;
            video.CreatedBy = CreatedBy;
            video.CreatedDate = CreatedDate;
            video.ModifiedBy = ModifiedBy;
            video.ModifiedDate = ModifiedDate;

            video.Name = Name;
            video.Description = Description;
            video.SortOrder = SortOrder;
            video.UploadDate = UploadDate;
            video.ThumbnailUrl = ThumbnailUrl;
            video.ContentUrl = ContentUrl;
            video.EmbedUrl = EmbedUrl;
            video.Duration = Duration;
            video.LanguageCode = LanguageCode;
            video.OwnerId = OwnerId;
            video.OwnerType = OwnerType;

            return video;
        }

        public virtual VideoEntity FromModel(Video video, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(video);

            pkMap.AddPair(video, this);

            Id = video.Id;
            CreatedBy = video.CreatedBy;
            CreatedDate = video.CreatedDate;
            ModifiedBy = video.ModifiedBy;
            ModifiedDate = video.ModifiedDate;

            Name = video.Name;
            Description = video.Description;
            SortOrder = video.SortOrder;
            UploadDate = video.UploadDate.GetValueOrDefault();
            ThumbnailUrl = video.ThumbnailUrl;
            ContentUrl = video.ContentUrl;
            EmbedUrl = video.EmbedUrl;
            Duration = video.Duration;
            LanguageCode = video.LanguageCode;
            OwnerId = video.OwnerId;
            OwnerType = video.OwnerType;

            return this;
        }

        public virtual void Patch(VideoEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.Name = Name;
            target.Description = Description;
            target.SortOrder = SortOrder;
            target.UploadDate = UploadDate;
            target.ThumbnailUrl = ThumbnailUrl;
            target.ContentUrl = ContentUrl;
            target.EmbedUrl = EmbedUrl;
            target.Duration = Duration;
            target.LanguageCode = LanguageCode;
            target.OwnerId = OwnerId;
            target.OwnerType = OwnerType;
        }
    }
}
