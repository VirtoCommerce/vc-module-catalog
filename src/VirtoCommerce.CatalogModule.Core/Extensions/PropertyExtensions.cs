using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    public static class PropertyExtensions
    {
        public static bool HasPropertyExcluded(this IHasExcludedProperties obj, string propertyName)
        {
            return obj.ExcludedProperties?.Any(x => x.Name.EqualsInvariant(propertyName)) == true;
        }

        public static void InheritExcludedProperties<T>(this T obj, IHasExcludedProperties parent)
            where T : IHasProperties, IHasExcludedProperties
        {
            if (parent == null)
            {
                return;
            }

            if (parent.ExcludedProperties?.Any() == true)
            {
                // Add the excluded properties as inherited excluded properties
                var inheritedExcludedProperties = parent.ExcludedProperties.Select(x => x.Inherit()).ToArray();
                obj.ExcludedProperties ??= new List<ExcludedProperty>();
                obj.ExcludedProperties.AddDistinct(inheritedExcludedProperties);
            }

            if (obj.Properties != null && obj.ExcludedProperties?.Any() == true)
            {
                obj.Properties = obj.Properties.Where(x => !obj.HasPropertyExcluded(x.Name)).ToList();
            }
        }
    }
}
