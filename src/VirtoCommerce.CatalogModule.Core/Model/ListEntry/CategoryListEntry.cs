using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.ListEntry
{
    /// <summary>
    /// Category ListEntry record.
    /// </summary>
    public class CategoryListEntry : ListEntryBase
    {
        public const string TypeName = "category";

        public override ListEntryBase FromModel(AuditableEntity entity)
        {
            base.FromModel(entity);

            if (entity is Category category)
            {
                Type = "category";
                ImageUrl = category.ImgSrc;
                Code = category.Code;
                Name = category.Name;
                IsActive = category.IsActive;
                Links = category.Links;
                CatalogId = category.CatalogId;
            }

            return this;
        }
    }
}
