using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Web.Model
{
    public class AggregationProperty : ICloneable
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public int? Size { get; set; }
        public int ValuesCount => Values?.Count ?? 0;
        public IList<string> Values { get; set; }

        public object Clone()
        {
            var result = (AggregationProperty) MemberwiseClone();
            result.Values = Values?.ToList();
            return result;
        }
    }
}
