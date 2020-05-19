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
                    result = Convert.ToString(value);
                    break;
                case PropertyValueType.ShortText:
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
                case PropertyValueType.GeoPoint:
                    result = Convert.ToString(value);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}
