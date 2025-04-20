using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public ObservableCollection<MeasureUnitLocalizedNameEntity> LocalizedNames { get; set; }
            = new NullCollection<MeasureUnitLocalizedNameEntity>();

        [Required]
        [StringLength(64)]
        public string Symbol { get; set; }

        public ObservableCollection<MeasureUnitLocalizedSymbolEntity> LocalizedSymbols { get; set; }
            = new NullCollection<MeasureUnitLocalizedSymbolEntity>();

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

            if (LocalizedNames != null)
            {
                measureUnit.LocalizedName = new LocalizedString();
                foreach (var localizedName in LocalizedNames)
                {
                    measureUnit.LocalizedName.SetValue(localizedName.LanguageCode, localizedName.Value);
                }
            }

            if (LocalizedSymbols != null)
            {
                measureUnit.LocalizedSymbol = new LocalizedString();
                foreach (var localizedSymbol in LocalizedSymbols)
                {
                    measureUnit.LocalizedSymbol.SetValue(localizedSymbol.LanguageCode, localizedSymbol.Value);
                }
            }

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

            if (measureUnit.LocalizedName != null)
            {
                LocalizedNames = new ObservableCollection<MeasureUnitLocalizedNameEntity>(measureUnit.LocalizedName.Values
                    .Select(x =>
                    {
                        var entity = AbstractTypeFactory<MeasureUnitLocalizedNameEntity>.TryCreateInstance();
                        entity.LanguageCode = x.Key;
                        entity.Value = x.Value;
                        return entity;
                    }));
            }

            if (measureUnit.LocalizedSymbol != null)
            {
                LocalizedSymbols = new ObservableCollection<MeasureUnitLocalizedSymbolEntity>(measureUnit.LocalizedSymbol.Values
                    .Select(x =>
                    {
                        var entity = AbstractTypeFactory<MeasureUnitLocalizedSymbolEntity>.TryCreateInstance();
                        entity.LanguageCode = x.Key;
                        entity.Value = x.Value;
                        return entity;
                    }));
            }

            return this;
        }

        public void Patch(MeasureUnitEntity target)
        {
            target.Code = Code;
            target.Name = Name;
            target.Symbol = Symbol;
            target.ConversionFactor = ConversionFactor;
            target.IsDefault = IsDefault;

            if (!LocalizedNames.IsNullCollection())
            {
                var localizedNameComparer = AnonymousComparer.Create((MeasureUnitLocalizedNameEntity x) => $"{x.Value}-{x.LanguageCode}");
                LocalizedNames.Patch(target.LocalizedNames, localizedNameComparer, (sourceValue, targetValue) => { });
            }

            if (!LocalizedSymbols.IsNullCollection())
            {
                var localizedNameComparer = AnonymousComparer.Create((MeasureUnitLocalizedSymbolEntity x) => $"{x.Value}-{x.LanguageCode}");
                LocalizedSymbols.Patch(target.LocalizedSymbols, localizedNameComparer, (sourceValue, targetValue) => { });
            }
        }
    }
}
