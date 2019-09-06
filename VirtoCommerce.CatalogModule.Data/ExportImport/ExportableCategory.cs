using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCategory : Category, IExportable
    {
        public virtual ExportableCategory FromModel(Category category)
        {
            Id = category.Id;
            CreatedDate = category.CreatedDate;
            ModifiedDate = category.ModifiedDate;
            CreatedBy = category.CreatedBy;
            ModifiedBy = category.ModifiedBy;
            SeoInfos = category.SeoInfos;
            Links = category.Links;
            PropertyValues = category.PropertyValues;
            Properties = category.Properties;
            Children = category.Children;
            IsActive = category.IsActive;
            Priority = category.Priority;
            PackageType = category.PackageType;
            Parents = category.Parents;
            Level = category.Level;
            IsVirtual = category.IsVirtual;
            Path = category.Path;
            Name = category.Name;
            TaxType = category.TaxType;
            Code = category.Code;
            ParentId = category.ParentId;
            Catalog = category.Catalog;
            CatalogId = category.CatalogId;
            Images = category.Images;
            Outlines = category.Outlines;

            return this;
        }
    }
}
