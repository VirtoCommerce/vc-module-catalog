using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableProperty : Property, IExportable
    {
        public virtual ExportableProperty FromModel(Property property)
        {
            Id = property.Id;
            CreatedDate = property.CreatedDate;
            ModifiedDate = property.ModifiedDate;
            CreatedBy = property.CreatedBy;
            ModifiedBy = property.ModifiedBy;
            Dictionary = property.Dictionary;
            DisplayNames = property.DisplayNames;
            Attributes = property.Attributes;
            Type = property.Type;
            ValueType = property.ValueType;
            Hidden = property.Hidden;
            Multilanguage = property.Multilanguage;
            Multivalue = property.Multivalue;
            IsInherited = property.IsInherited;
            Required = property.Required;
            Name = property.Name;
            Category = property.Category;
            CategoryId = property.CategoryId;
            Catalog = property.Catalog;
            CatalogId = property.CatalogId;
            ValidationRules = property.ValidationRules;

            return this;
        }
    }
}
