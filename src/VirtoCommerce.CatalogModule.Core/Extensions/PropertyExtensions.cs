using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    public static class PropertyExtensions
    {
        public static bool HasPropertyExcluded(this IHasExcludedProperties obj, string propertyName)
        {
            return obj.ExcludedProperties?.Any(x => x.Name.EqualsIgnoreCase(propertyName)) == true;
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

        public static void SanitizePropertyValues(this IEnumerable<IHasProperties> propertyOwners, IPropertyValueSanitizer propertyValueSanitizer)
        {
            foreach (var propertyOwner in propertyOwners)
            {
                propertyOwner.Properties.SanitizeValues(propertyValueSanitizer);
            }
        }

        public static void SanitizeValues(this IEnumerable<Property> properties, IPropertyValueSanitizer propertyValueSanitizer)
        {
            if (properties.IsNullOrEmpty())
            {
                return;
            }

            foreach (var property in properties)
            {
                if (property.ValueType == PropertyValueType.Html)
                {
                    foreach (var propertyValue in property.Values)
                    {
                        propertyValue.Value = propertyValueSanitizer.Sanitize(propertyValue.Value?.ToString());
                    }
                }
            }
        }
    }
}
