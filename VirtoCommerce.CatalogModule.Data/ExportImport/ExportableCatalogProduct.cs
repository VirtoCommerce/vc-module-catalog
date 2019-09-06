using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCatalogProduct : CatalogProduct, IExportable
    {
        new public virtual string SeoObjectType { get; set; }

        public static ExportableCatalogProduct FromModel(CatalogProduct product)
        {
            var result = new ExportableCatalogProduct
            {
                Id = product.Id,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate,
                CreatedBy = product.CreatedBy,
                ModifiedBy = product.ModifiedBy,
                Length = product.Length,
                MaxNumberOfDownload = product.MaxNumberOfDownload,
                DownloadExpiration = product.DownloadExpiration,
                DownloadType = product.DownloadType,
                HasUserAgreement = product.HasUserAgreement,
                ShippingType = product.ShippingType,
                TaxType = product.TaxType,
                Vendor = product.Vendor,
                StartDate = product.StartDate,
                EndDate = product.EndDate,
                Priority = product.Priority,
                Properties = product.Properties,
                PropertyValues = product.PropertyValues,
                Images = product.Images,
                Assets = product.Assets,
                Links = product.Links,
                Variations = product.Variations,
                SeoObjectType = product.SeoObjectType,
                SeoInfos = product.SeoInfos,
                Reviews = product.Reviews,
                Associations = product.Associations,
                ReferencedAssociations = product.ReferencedAssociations,
                Prices = product.Prices,
                EnableReview = product.EnableReview,
                Width = product.Width,
                Outlines = product.Outlines,
                Height = product.Height,
                Code = product.Code,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                Gtin = product.Gtin,
                Name = product.Name,
                CatalogId = product.CatalogId,
                Catalog = product.Catalog,
                CategoryId = product.CategoryId,
                Category = product.Category,
                MainProductId = product.MainProductId,
                Inventories = product.Inventories,
                MainProduct = product.MainProduct,
                IsActive = product.IsActive,
                TrackInventory = product.TrackInventory,
                IndexingDate = product.IndexingDate,
                MaxQuantity = product.MaxQuantity,
                MinQuantity = product.MinQuantity,
                ProductType = product.ProductType,
                PackageType = product.PackageType,
                WeightUnit = product.WeightUnit,
                Weight = product.Weight,
                MeasureUnit = product.MeasureUnit,
                IsBuyable = product.IsBuyable
            };

            return result;
        }
    }
}
