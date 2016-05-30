using System;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace VirtoCommerce.CatalogModule.Web.ExportImport
{
    public class ExpressionConverter<T> : ITypeConverter
    {
        public Func<string, T> InExpression { get; set; }
        public Func<T, string> OutExpression { get; set; }
        public ExpressionConverter(Func<string, T> inExp, Func<T, string> outExp)
        {
            InExpression = inExp;
            OutExpression = outExp;
        }
        public bool CanConvertFrom(Type type)
        {
            return InExpression != null;
        }

        public bool CanConvertTo(Type type)
        {
            return OutExpression != null;
        }

        [CLSCompliant(false)]
        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            return InExpression(text);
        }

        [CLSCompliant(false)]
        public string ConvertToString(TypeConverterOptions options, object value)
        {
            return OutExpression((T)value);
        }
    }

    public static class CsvHelperExtensions
    {
        [CLSCompliant(false)]
        public static CsvPropertyMap UsingExpression<T>(this CsvPropertyMap map, Func<string, T> readExpression,
            Func<T, string> writeExpression)
        {
            return map.TypeConverter(new ExpressionConverter<T>(readExpression, writeExpression));
        }
    }
}
