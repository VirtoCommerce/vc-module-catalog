using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class MeasureEntity : AuditableEntity, IDataEntity<MeasureEntity, Measure>
    {
        [Required]
        [StringLength(64)]
        public string Code { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ObservableCollection<MeasureUnitEntity> Units { get; set; } = new NullCollection<MeasureUnitEntity>();

        public Measure ToModel(Measure measure)
        {
            measure.Id = Id;
            measure.CreatedBy = CreatedBy;
            measure.CreatedDate = CreatedDate;
            measure.ModifiedBy = ModifiedBy;
            measure.ModifiedDate = ModifiedDate;

            measure.Code = Code;
            measure.Name = Name;
            measure.Description = Description;

            measure.Units = Units.Select(x => x.ToModel(AbstractTypeFactory<MeasureUnit>.TryCreateInstance())).ToList();

            return measure;
        }

        public MeasureEntity FromModel(Measure measure, PrimaryKeyResolvingMap pkMap)
        {
            pkMap.AddPair(measure, this);

            Id = measure.Id;
            CreatedBy = measure.CreatedBy;
            CreatedDate = measure.CreatedDate;
            ModifiedBy = measure.ModifiedBy;
            ModifiedDate = measure.ModifiedDate;

            Code = measure.Code;
            Name = measure.Name;
            Description = measure.Description;

            if (measure.Units != null)
            {
                Units = new ObservableCollection<MeasureUnitEntity>(measure.Units.Select(x => AbstractTypeFactory<MeasureUnitEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public void Patch(MeasureEntity target)
        {
            target.Code = Code;
            target.Name = Name;
            target.Description = Description;

            if (!Units.IsNullCollection())
            {
                Units.Patch(target.Units, (sourceUnit, targetUnit) => sourceUnit.Patch(targetUnit));
            }
        }
    }
}
