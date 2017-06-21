using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class EditorialReviewEntity : AuditableEntity
    {
        public int Priority { get; set; }

        [StringLength(128)]
        public string Source { get; set; }

        public string Content { get; set; }

        [Required]
        public int ReviewState { get; set; }

        public string Comments { get; set; }

        [StringLength(64)]
        public string Locale { get; set; }

        #region Navigation Properties
        public string ItemId { get; set; }

        public ItemEntity CatalogItem { get; set; }
        #endregion

        public virtual EditorialReview ToModel(EditorialReview review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            review.Id = this.Id;
            review.Content = this.Content;
            review.CreatedBy = this.CreatedBy;
            review.CreatedDate = this.CreatedDate;
            review.ModifiedBy = this.ModifiedBy;
            review.ModifiedDate = this.ModifiedDate;
            review.LanguageCode = this.Locale;
            review.ReviewType = this.Source;

            return review;
        }

        public virtual EditorialReviewEntity FromModel(EditorialReview review, PrimaryKeyResolvingMap pkMap)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            pkMap.AddPair(review, this);

            this.Id = review.Id;
            this.Content = review.Content;
            this.CreatedBy = review.CreatedBy;
            this.CreatedDate = review.CreatedDate;
            this.ModifiedBy = review.ModifiedBy;
            this.ModifiedDate = review.ModifiedDate;
            this.Locale = review.LanguageCode;
            this.Source = review.ReviewType;

            return this;
        }

        public virtual void Patch(EditorialReviewEntity target)
        {
            target.Content = this.Content;
            target.Locale = this.Locale;
            target.Source = this.Source;
        }
    }
}
