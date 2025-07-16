using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using static VirtoCommerce.Platform.Data.Infrastructure.DbContextBase;

namespace VirtoCommerce.CatalogModule.Data.Model;

public class AutomaticLinkQueryEntity : AuditableEntity, IDataEntity<AutomaticLinkQueryEntity, AutomaticLinkQuery>
{
    [Required]
    [StringLength(IdLength)]
    public string TargetCategoryId { get; set; }

    [StringLength(IdLength)]
    public string SourceCatalogId { get; set; }

    [StringLength(Length2048)]
    public string SourceCatalogQuery { get; set; }

    // Navigation properties
    public virtual CategoryEntity TargetCategory { get; set; }
    public virtual CatalogEntity SourceCatalog { get; set; }

    public virtual AutomaticLinkQuery ToModel(AutomaticLinkQuery model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.TargetCategoryId = TargetCategoryId;
        model.SourceCatalogId = SourceCatalogId;
        model.SourceCatalogQuery = SourceCatalogQuery;

        return model;
    }

    public virtual AutomaticLinkQueryEntity FromModel(AutomaticLinkQuery model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        TargetCategoryId = model.TargetCategoryId;
        SourceCatalogId = model.SourceCatalogId;
        SourceCatalogQuery = model.SourceCatalogQuery;

        return this;
    }

    public virtual void Patch(AutomaticLinkQueryEntity target)
    {
        target.SourceCatalogId = SourceCatalogId;
        target.SourceCatalogQuery = SourceCatalogQuery;
    }
}
