using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class MeasureUnitEntity : AuditableEntity, IDataEntity<MeasureUnitEntity, MeasureUnit>
    {
        [Required]
        [StringLength(64)]
        public string Code { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [StringLength(64)]
        public string Symbol { get; set; }

        public decimal ConversionFactor { get; set; }

        public bool IsDefault { get; set; }

        public string MeasureId { get; set; }
        public virtual MeasureEntity Measure { get; set; }

        public MeasureUnit ToModel(MeasureUnit measureUnit)
        {
            measureUnit.Id = Id;
            measureUnit.CreatedBy = CreatedBy;
            measureUnit.CreatedDate = CreatedDate;
            measureUnit.ModifiedBy = ModifiedBy;
            measureUnit.ModifiedDate = ModifiedDate;

            measureUnit.Code = Code;
            measureUnit.Name = Name;
            measureUnit.Symbol = Symbol;
            measureUnit.ConversionFactor = ConversionFactor;
            measureUnit.IsDefault = IsDefault;

            return measureUnit;
        }

        public MeasureUnitEntity FromModel(MeasureUnit measureUnit, PrimaryKeyResolvingMap pkMap)
        {
            pkMap.AddPair(measureUnit, this);

            Id = measureUnit.Id;
            CreatedBy = measureUnit.CreatedBy;
            CreatedDate = measureUnit.CreatedDate;
            ModifiedBy = measureUnit.ModifiedBy;
            ModifiedDate = measureUnit.ModifiedDate;

            Code = measureUnit.Code;
            Name = measureUnit.Name;
            Symbol = measureUnit.Symbol;
            ConversionFactor = measureUnit.ConversionFactor;
            IsDefault = measureUnit.IsDefault;

            return this;
        }

        public void Patch(MeasureUnitEntity target)
        {
            target.Code = Code;
            target.Name = Name;
            target.Symbol = Symbol;
            target.ConversionFactor = ConversionFactor;
            target.IsDefault = IsDefault;
        }
    }
}
