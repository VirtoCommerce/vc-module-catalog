using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.ListEntry
{
    /// <summary>
    /// Product ListEntry record.
    /// </summary>
    public class ProductListEntry : ListEntryBase
    {
        public const string TypeName = "product";
        public string ProductType { get; set; }

        public override ListEntryBase FromModel(AuditableEntity entity)
        {
            base.FromModel(entity);

            if (entity is CatalogProduct product)
            {
                Type = "product";
                ImageUrl = product.ImgSrc;
                Code = product.Code;
                Name = product.Name;
                IsActive = product.IsActive ?? true;
                ProductType = product.ProductType;
                Links = product.Links;
                CatalogId = product.CatalogId;
            }

            return this;
        }
    }
}
