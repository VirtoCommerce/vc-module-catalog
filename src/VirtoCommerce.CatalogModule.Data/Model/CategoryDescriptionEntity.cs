using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CategoryDescriptionEntity : AuditableEntity, IDataEntity<CategoryDescriptionEntity, CategoryDescription>
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

        public string CategoryId { get; set; }
        public CategoryEntity Category { get; set; }

        #endregion

        public virtual CategoryDescription ToModel(CategoryDescription description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            description.Id = Id;
            description.Content = Content;
            description.CreatedBy = CreatedBy;
            description.CreatedDate = CreatedDate;
            description.ModifiedBy = ModifiedBy;
            description.ModifiedDate = ModifiedDate;

            description.LanguageCode = Locale;
            description.DescriptionType = Source;

            return description;
        }

        public virtual CategoryDescriptionEntity FromModel(CategoryDescription description, PrimaryKeyResolvingMap pkMap)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            pkMap.AddPair(description, this);

            Id = description.Id;
            Content = description.Content;
            CreatedBy = description.CreatedBy;
            CreatedDate = description.CreatedDate;
            ModifiedBy = description.ModifiedBy;
            ModifiedDate = description.ModifiedDate;

            Locale = description.LanguageCode;
            Source = description.DescriptionType;

            return this;
        }

        public virtual void Patch(CategoryDescriptionEntity target)
        {
            target.Content = Content;
            target.Locale = Locale;
            target.Source = Source;
        }
    }
}

