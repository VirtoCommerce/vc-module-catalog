using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class ImageEntity : AuditableEntity, IHasOuterId
    {
        [StringLength(2083)]
        [Required]
        public string Url { get; set; }

        [StringLength(1024)]
        public string Name { get; set; }

        [StringLength(5)]
        public string LanguageCode { get; set; }

        [StringLength(64)]
        public string Group { get; set; }
        public int SortOrder { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [StringLength(1024)]
        public string AltText { get; set; }

        #region Navigation Properties

        public string ItemId { get; set; }
        public virtual ItemEntity CatalogItem { get; set; }

        public string CategoryId { get; set; }
        public virtual CategoryEntity Category { get; set; }

        #endregion

        public virtual Image ToModel(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            image.Id = Id;
            image.CreatedBy = CreatedBy;
            image.CreatedDate = CreatedDate;
            image.ModifiedBy = ModifiedBy;
            image.ModifiedDate = ModifiedDate;
            image.OuterId = OuterId;

            image.Group = Group;
            image.LanguageCode = LanguageCode;
            image.Name = Name;
            image.SortOrder = SortOrder;
            image.Url = Url;
            image.RelativeUrl = Url;
            image.Description = Description;
            image.AltText = AltText;

            if (!(string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(LanguageCode) && string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(AltText)))
            {
                var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                seoInfo.Name = Name;
                seoInfo.MetaDescription = Description;
                seoInfo.LanguageCode = LanguageCode;
                seoInfo.SemanticUrl = Url;
                seoInfo.ImageAltDescription = AltText;
                image.SeoInfos = new List<SeoInfo> { seoInfo };
            }


            return image;
        }

        public virtual ImageEntity FromModel(Image image, PrimaryKeyResolvingMap pkMap)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            pkMap.AddPair(image, this);

            Id = image.Id;
            CreatedBy = image.CreatedBy;
            CreatedDate = image.CreatedDate;
            ModifiedBy = image.ModifiedBy;
            ModifiedDate = image.ModifiedDate;
            OuterId = image.OuterId;

            Group = image.Group;
            LanguageCode = image.LanguageCode;
            Name = image.Name;
            SortOrder = image.SortOrder;
            AltText = image.AltText;
            Description = image.Description;
            Url = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;

            return this;
        }

        public virtual void Patch(ImageEntity target)
        {
            target.LanguageCode = LanguageCode;
            target.Name = Name;
            target.Group = Group;
            target.SortOrder = SortOrder;
            target.Url = Url;
            target.AltText = AltText;
            target.Description = Description;
        }
    }
}
