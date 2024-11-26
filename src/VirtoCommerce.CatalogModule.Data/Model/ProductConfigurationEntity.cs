using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model;

public class ProductConfigurationEntity : AuditableEntity, IDataEntity<ProductConfigurationEntity, ProductConfiguration>
{
    [Required]
    [StringLength(128)]
    public string ProductId { get; set; }

    public bool IsActive { get; set; }

    public virtual ItemEntity Product { get; set; }
    public virtual ObservableCollection<ProductConfigurationSectionEntity> Sections { get; set; } = new NullCollection<ProductConfigurationSectionEntity>();

    public virtual ProductConfiguration ToModel(ProductConfiguration model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.ProductId = ProductId;
        model.IsActive = IsActive;

        model.Sections = Sections.Select(x => x.ToModel(AbstractTypeFactory<ProductConfigurationSection>.TryCreateInstance())).ToList();

        return model;
    }

    public virtual ProductConfigurationEntity FromModel(ProductConfiguration model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        ProductId = model.ProductId;
        IsActive = model.IsActive;

        if (model.Sections != null)
        {
            Sections = new ObservableCollection<ProductConfigurationSectionEntity>(model.Sections.Select(x => AbstractTypeFactory<ProductConfigurationSectionEntity>.TryCreateInstance().FromModel(x, pkMap)));
        }

        return this;
    }

    public virtual void Patch(ProductConfigurationEntity target)
    {
        target.ProductId = ProductId;
        target.IsActive = IsActive;

        if (!Sections.IsNullCollection())
        {
            Sections.Patch(target.Sections, (sourceSection, targetSection) => sourceSection.Patch(targetSection));
        }
    }
}
