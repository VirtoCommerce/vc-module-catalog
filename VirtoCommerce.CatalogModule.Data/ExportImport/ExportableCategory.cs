using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCategory : Category, IExportable
    {
        new public virtual string SeoObjectType { get; set; }

        public static ExportableCategory FromModel(Category category)
        {
            var result = new ExportableCategory
            {
                Id = category.Id,
                CreatedDate = category.CreatedDate,
                ModifiedDate = category.ModifiedDate,
                CreatedBy = category.CreatedBy,
                ModifiedBy = category.ModifiedBy,
                SeoInfos = category.SeoInfos,
                SeoObjectType = category.SeoObjectType,
                Links = category.Links,
                PropertyValues = category.PropertyValues,
                Properties = category.Properties,
                Children = category.Children,
                IsActive = category.IsActive,
                Priority = category.Priority,
                PackageType = category.PackageType,
                Parents = category.Parents,
                Level = category.Level,
                IsVirtual = category.IsVirtual,
                Path = category.Path,
                Name = category.Name,
                TaxType = category.TaxType,
                Code = category.Code,
                ParentId = category.ParentId,
                Catalog = category.Catalog,
                CatalogId = category.CatalogId,
                Images = category.Images,
                Outlines = category.Outlines
            };
            return result;
        }
    }
}
