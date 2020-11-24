using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class ExcludedProperty
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

        public override bool Equals(object obj)
        {
            if (obj is ExcludedProperty excludedProperty)
            {
                return Name.EqualsInvariant(excludedProperty.Name);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
