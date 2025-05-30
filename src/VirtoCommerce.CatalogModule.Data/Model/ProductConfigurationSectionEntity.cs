using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model;

public class ProductConfigurationSectionEntity : AuditableEntity, IDataEntity<ProductConfigurationSectionEntity, ProductConfigurationSection>
{
    [Required]
    [StringLength(128)]
    public string ConfigurationId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; }

    [Required]
    [StringLength(64)]
    public string Type { get; set; }

    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowCustomText { get; set; }
    public bool AllowPredefinedOptions { get; set; }
    public int DisplayOrder { get; set; }

    public virtual ProductConfigurationEntity Configuration { get; set; }
    public virtual ObservableCollection<ProductConfigurationOptionEntity> Options { get; set; } = new NullCollection<ProductConfigurationOptionEntity>();

    public virtual ProductConfigurationSection ToModel(ProductConfigurationSection model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.ConfigurationId = ConfigurationId;
        model.Name = Name;
        model.Description = Description;
        model.IsRequired = IsRequired;
        model.AllowCustomText = AllowCustomText;
        model.AllowPredefinedOptions = AllowPredefinedOptions;
        model.DisplayOrder = DisplayOrder;
        model.Type = Type;

        model.Options = Options.Select(x => x.ToModel(AbstractTypeFactory<ProductConfigurationOption>.TryCreateInstance())).ToList();

        return model;
    }

    public virtual ProductConfigurationSectionEntity FromModel(ProductConfigurationSection model, PrimaryKeyResolvingMap pkMap)
    {
        ArgumentNullException.ThrowIfNull(model);

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        ConfigurationId = model.ConfigurationId;
        Name = model.Name;
        Description = model.Description;
        IsRequired = model.IsRequired;
        AllowCustomText = model.AllowCustomText;
        AllowPredefinedOptions = model.AllowPredefinedOptions;
        DisplayOrder = model.DisplayOrder;
        Type = model.Type;

        if (model.Options != null)
        {
            Options = new ObservableCollection<ProductConfigurationOptionEntity>(model.Options.Select(x => AbstractTypeFactory<ProductConfigurationOptionEntity>.TryCreateInstance().FromModel(x, pkMap)));
        }

        return this;
    }

    public virtual void Patch(ProductConfigurationSectionEntity target)
    {
        target.Name = Name;
        target.Description = Description;
        target.IsRequired = IsRequired;
        target.AllowCustomText = AllowCustomText;
        target.AllowPredefinedOptions = AllowPredefinedOptions;
        target.DisplayOrder = DisplayOrder;
        target.Type = Type;

        if (!Options.IsNullCollection())
        {
            Options.Patch(target.Options, (sourceOption, targetOption) => sourceOption.Patch(targetOption));
        }
    }
}
