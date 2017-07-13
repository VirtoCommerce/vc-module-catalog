using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class PropertiesExtentions
    {
        /// <summary>
        /// Copies properties objects form preloaded model (categories etc) object to objects without properties
        /// </summary>
        public static void CopyProperties(this IHasProperties[] preloadedModels, IHasProperties[] targetModels)
        {
            var preloadedProperties = preloadedModels.SelectMany(x => x.Properties).ToList();
            foreach (var catalog in targetModels.Where(x=>x.PropertyValues != null))
            {
                foreach (var propertyValue in catalog.PropertyValues)
                {
                    var property = preloadedProperties.FirstOrDefault(x => x.Id == propertyValue.PropertyId);
                    if (property == null)
                        continue;

                    if (property.ValidationRules != null)
                    {
                        foreach (var validationRule in property.ValidationRules)
                        {
                            if (validationRule.Property == null)
                            {
                                validationRule.Property = property;
                            }
                        }
                    }

                    propertyValue.Property = property;
                    if (catalog.Properties == null)
                    {
                        catalog.Properties = new List<Property>();
                    }

                    if (catalog.Properties.All(x => x.Id != property.Id))
                    {
                        catalog.Properties.Add(property);
                    }
                }
            }
        }
    }
}
