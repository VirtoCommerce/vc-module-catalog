using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model;

public class PropertyGroupEntity : AuditableEntity, IDataEntity<PropertyGroupEntity, PropertyGroup>
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    public string CatalogId { get; set; }
    public virtual CatalogEntity Catalog { get; set; }

    public ObservableCollection<PropertyGroupLocalizedNameEntity> LocalizedNames { get; set; }
        = new NullCollection<PropertyGroupLocalizedNameEntity>();

    public ObservableCollection<PropertyGroupLocalizedDescriptionEntity> LocalizedDescriptions { get; set; }
        = new NullCollection<PropertyGroupLocalizedDescriptionEntity>();

    public int DisplayOrder { get; set; }

    public PropertyGroup ToModel(PropertyGroup model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Name = Name;
        model.DisplayOrder = DisplayOrder;
        model.CatalogId = CatalogId;

        if (LocalizedNames != null)
        {
            model.LocalizedName = new LocalizedString();
            foreach (var localizedName in LocalizedNames)
            {
                model.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
            }
        }

        if (LocalizedDescriptions != null)
        {
            model.LocalizedDescription = new LocalizedString();
            foreach (var localizedDescription in LocalizedDescriptions)
            {
                model.LocalizedDescription.SetValue(localizedDescription.LanguageCode, localizedDescription.Value);
            }
        }

        return model;
    }

    public PropertyGroupEntity FromModel(PropertyGroup model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        Name = model.Name;
        DisplayOrder = model.DisplayOrder;
        CatalogId = model.CatalogId;

        if (model.LocalizedName != null)
        {
            LocalizedNames = new ObservableCollection<PropertyGroupLocalizedNameEntity>(model.LocalizedName.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<PropertyGroupLocalizedNameEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                }));
        }

        if (model.LocalizedDescription != null)
        {
            LocalizedDescriptions = new ObservableCollection<PropertyGroupLocalizedDescriptionEntity>(model.LocalizedDescription.Values
                .Select(x =>
                {
                    var entity = AbstractTypeFactory<PropertyGroupLocalizedDescriptionEntity>.TryCreateInstance();
                    entity.LanguageCode = x.Key;
                    entity.Value = x.Value;
                    return entity;
                }));
        }

        return this;
    }

    public void Patch(PropertyGroupEntity target)
    {
        target.Name = Name;
        target.DisplayOrder = DisplayOrder;
        target.CatalogId = CatalogId;

        if (!LocalizedNames.IsNullCollection())
        {
            var localizedNameComparer = AnonymousComparer.Create((PropertyGroupLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (sourceValue, targetValue) => { });
        }

        if (!LocalizedDescriptions.IsNullCollection())
        {
            var localizedDescriptionComparer = AnonymousComparer.Create((PropertyGroupLocalizedDescriptionEntity x) => $"{x.Value}-{x.LanguageCode}");
            LocalizedDescriptions.Patch(target.LocalizedDescriptions, localizedDescriptionComparer, (sourceValue, targetValue) => { });
        }
    }
}
