using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class ExcludedProperty : ValueObject
    {
        public string Name { get; set; }

        public bool IsInherited { get; set; }

        public ExcludedProperty()
        {
        }

        public ExcludedProperty(string name)
        {
            Name = name;
        }

        public ExcludedProperty Inherit()
        {
            return IsInherited ? this : new ExcludedProperty(Name) { IsInherited = true };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
