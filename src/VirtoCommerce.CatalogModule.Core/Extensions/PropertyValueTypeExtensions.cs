using System;
using System.Globalization;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Extensions
{
    public static class PropertyValueTypeExtensions
    {
        public static object ConvertValue(this PropertyValueType valueType, object value)
        {
            object result;

            switch (valueType)
            {
                case PropertyValueType.LongText:
                case PropertyValueType.ShortText:
                case PropertyValueType.GeoPoint:
                case PropertyValueType.Html:
                    result = Convert.ToString(value);
                    break;
                case PropertyValueType.Number:
                    try
                    {
                        result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        result = Convert.ToString(value, CultureInfo.InstalledUICulture);
                    }
                    break;
                case PropertyValueType.DateTime:
                    result = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                    break;
                case PropertyValueType.Boolean:
                    result = Convert.ToBoolean(value);
                    break;
                case PropertyValueType.Integer:
                    result = Convert.ToInt32(value);
                    break;
                case PropertyValueType.Measure:
                    result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}
