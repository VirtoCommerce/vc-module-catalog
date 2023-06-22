using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    public static class ProductExtensions
    {
        private static readonly string[] AcceptableProperties =
        {
            "Code", "DownloadExpiration", "DownloadType", "EnableReview", "EndDate", "Gtin", "HasUserAgreement",
            "Height", "IsActive", "IsBuyable", "Length", "ManufacturerPartNumber",
            "MaxNumberOfDownload", "MaxQuantity", "MeasureUnit", "MinQuantity", "Name", "OuterId", "Priority",
            "ShippingType", "StartDate", "TaxType", "TitularItemId", "TrackInventory", "Vendor",
            "Weight", "WeightUnit", "Width"
        };

        public static void ApplyTo(this JObject source, CatalogProduct product, ModelStateDictionary modelState, string language)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            foreach (var property in source.Properties())
            {
                if (Array.IndexOf(AcceptableProperties, property.Name) == -1)
                {
                    if (!SetDynamicProperty(product, property, language))
                    {
                        modelState.AddModelError(property.Name, $"Property '{property.Name}' is not allowed or language '{language}' is incorrect.");
                    }
                }
                else
                {
                    if (!SetProperty(product, property))
                    {
                        modelState.AddModelError(property.Name, $"Property '{property.Name}' is invalid.");
                    }
                }
            }
        }

        private static bool SetDynamicProperty(CatalogProduct product, JProperty property, string language)
        {
            var prop = product.Properties.FirstOrDefault(x => x.Name == property.Name);
            if (prop != null)
            {
                var value = prop.Values.FirstOrDefault(x => x.LanguageCode == language);
                if (value != null)
                {
                    value.Value = property.Value;
                    return true;
                }
            }

            return false;
        }

        private static bool SetProperty(CatalogProduct product, JProperty property)
        {
            try
            {
                var prop = product.GetType().GetProperty(property.Name);
                prop.SetValue(product, ConverterHelper.Convert(prop, property.Value));
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
