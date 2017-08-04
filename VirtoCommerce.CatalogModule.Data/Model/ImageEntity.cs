using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class ImageEntity : AuditableEntity
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

            image.Id = this.Id;
            image.CreatedBy = this.CreatedBy;
            image.CreatedDate = this.CreatedDate;
            image.ModifiedBy = this.ModifiedBy;
            image.ModifiedDate = this.ModifiedDate;

            image.Group = this.Group;
            image.LanguageCode = this.LanguageCode;         
            image.Name = this.Name;
            image.SortOrder = this.SortOrder;
            image.Url = this.Url;

            return image;
        }

        public virtual ImageEntity FromModel(Image image, PrimaryKeyResolvingMap pkMap)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            pkMap.AddPair(image, this);

            this.Id = image.Id;
            this.CreatedBy = image.CreatedBy;
            this.CreatedDate = image.CreatedDate;
            this.ModifiedBy = image.ModifiedBy;
            this.ModifiedDate = image.ModifiedDate;

            this.Group = image.Group;
            this.LanguageCode = image.LanguageCode;
            this.Name = image.Name;
            this.SortOrder = image.SortOrder;
            this.Url = image.Url;

            return this;
        }

        public virtual void Patch(ImageEntity target)
        {
            target.LanguageCode = this.LanguageCode;
            target.Name = this.Name;
            target.Group = this.Group;
            target.SortOrder = this.SortOrder;
            target.Url = this.Url;
        }
    }
}
