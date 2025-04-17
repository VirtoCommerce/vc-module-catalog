using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model;

public class ProductConfigurationOptionEntity : AuditableEntity, IDataEntity<ProductConfigurationOptionEntity, ProductConfigurationOption>
{
    [Required]
    [StringLength(128)]
    public string SectionId { get; set; }

    [StringLength(128)]
    public string ProductId { get; set; }

    public int Quantity { get; set; }

    [StringLength(255)]
    public string Text { get; set; }

    public virtual ProductConfigurationSectionEntity Section { get; set; }
    public virtual ItemEntity Product { get; set; }

    public virtual ProductConfigurationOption ToModel(ProductConfigurationOption model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.SectionId = SectionId;
        model.ProductId = ProductId;
        model.Quantity = Quantity;
        model.Text = Text;

        if (Product != null)
        {
            model.Product = Product.ToModel(AbstractTypeFactory<Core.Model.CatalogProduct>.TryCreateInstance());
        }

        return model;
    }

    public virtual ProductConfigurationOptionEntity FromModel(ProductConfigurationOption model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        SectionId = model.SectionId;
        ProductId = model.ProductId;
        Quantity = model.Quantity;
        Text = model.Text;

        return this;
    }

    public virtual void Patch(ProductConfigurationOptionEntity target)
    {
        target.Quantity = Quantity;
        target.Text = Text;
    }
}
