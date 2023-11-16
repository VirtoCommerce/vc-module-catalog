using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Measure : AuditableEntity, ICloneable
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IList<MeasureUnit> Units { get; set; }

        public object Clone()
        {
            var result = (Measure)MemberwiseClone();

            result.Units = Units?.Select(x => x.Clone()).OfType<MeasureUnit>().ToList();

            return result;
        }
    }
}
