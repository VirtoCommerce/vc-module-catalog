using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model;
public class BrandStoreSettingEntity : AuditableEntity, IDataEntity<BrandStoreSettingEntity, BrandStoreSetting>
{
    public string StoreId { get; set; }

    public string BrandPropertyId { get; set; }

    public string BrandCatalogId { get; set; }

    public BrandStoreSettingEntity FromModel(BrandStoreSetting model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        StoreId = model.StoreId;
        BrandPropertyId = model.BrandPropertyId;
        BrandCatalogId = model.BrandCatalogId;

        return this;
    }

    public BrandStoreSetting ToModel(BrandStoreSetting model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.StoreId = StoreId;
        model.BrandPropertyId = BrandPropertyId;
        model.BrandCatalogId = BrandCatalogId;

        return model;
    }

    public void Patch(BrandStoreSettingEntity target)
    {
        target.StoreId = StoreId;
        target.BrandPropertyId = BrandPropertyId;
        target.BrandCatalogId = BrandCatalogId;
    }
}
