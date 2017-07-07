using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class ValidatoinExtentions
    {
        public static string[] FormatPropertyErrors(this ICollection<string> errors, PropertyValue propertyValue)
        {
            return errors.Select(error => $"{propertyValue.PropertyName}:{propertyValue.Value}:{error}").ToArray();
        }
    }
}
