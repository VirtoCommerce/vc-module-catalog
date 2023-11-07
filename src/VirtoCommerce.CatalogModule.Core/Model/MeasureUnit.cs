using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class MeasureUnit : AuditableEntity, ICloneable
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal ConversionFactor { get; set; }
        public bool IsDefault { get; set; }

        public object Clone()
        {
            var result = MemberwiseClone() as MeasureUnit;

            return result;
        }
    }
}
