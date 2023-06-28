using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    class PropertyValueProcessor
    {
        private Context _context;

        private PropertyValueProcessor(Context context)
        {
            _context = context;
        }

        public static PropertyValueProcessor Create(Context context)
        {
            var result = new PropertyValueProcessor(context);

            if (context.Property.Multivalue)
            {
                result._getValue = result.GetArray;
            }
            else
            {
                result._getValue = result.GetSingle;
            }

            if (context.Property.Dictionary)
            {
                result._convertValue = result.ConvertToString;
                result._mapValue = result.SearchInDictionary;
            }
            else
            {
                result._convertValue = result.ConvertToValue;
                result._mapValue = result.SkipMapping;
            }

            return result;
        }

        public void Process()
        {
            _getValue();
            _mapValue();
            SetValues();
        }

        private Action _getValue;
        private Func<JToken, object> _convertValue;
        private Action _mapValue;

        private void GetArray()
        {
            _context.ConvertedValues = _context.Value.Select(_convertValue);
        }

        private object ConvertToString(JToken rawValue)
        {
            return rawValue.ToString();
        }

        private void GetSingle()
        {
            _context.ConvertedValues = new[] { _convertValue(_context.Value) };
        }

        private object ConvertToValue(JToken rawValue)
        {
            return _context.Property.ValueType.ConvertValue(rawValue.ToString());
        }

        private void SearchInDictionary()
        {
            var allValues = _context.GetDictionaryItems;
            _context.MappedValues = _context.ConvertedValues
                .Select(x => new MappedValue
                {
                    DictionaryValue = allValues.FirstOrDefault(v => IsDictionaryItemAcceptable(v, (string)x)),
                    Value = x
                }).Where(x => x.DictionaryValue != null).ToList();
        }

        private bool IsDictionaryItemAcceptable(PropertyDictionaryItem v, string x)
        {
            return v.LocalizedValues.Any(l => l.LanguageCode == _context.Language && l.Value == x)
                   || v.Alias == x;
        }

        private void SkipMapping()
        {
            _context.MappedValues = _context.ConvertedValues.Select(x => new MappedValue { Value = x });
        }

        private void SetValues()
        {
            _context.Property.Values = _context.MappedValues.Select(x => new PropertyValue
            {
                Value = x.Value,
                LanguageCode = _context.Language,
                Alias = x.DictionaryValue?.Alias,
                ValueType = _context.Property.ValueType,
                ValueId = x.DictionaryValue?.Id,
                PropertyName = _context.Property.Name,
                //Id = ,
                //CreatedBy = ,
                //CreatedDate = ,
                //IsInherited = ,
                //ModifiedBy = ,
                //ModifiedDate = ,
                //OuterId = ,
                //Property = ,
                PropertyId = _context.Property.Id
            }).ToList();
        }

        internal class Context
        {
            public string Language { get; set; }
            public Property Property { get; set; }
            public JToken Value { get; set; }
            public IEnumerable<PropertyDictionaryItem> DictionaryItems { get; set; }
            internal IEnumerable<object> ConvertedValues { get; set; }
            internal IEnumerable<MappedValue> MappedValues { get; set; }

            internal IEnumerable<PropertyDictionaryItem> GetDictionaryItems =>
                DictionaryItems.Where(x => x.PropertyId == Property.Id).ToArray();
        }

        internal class MappedValue
        {
            public object Value { get; set; }
            public PropertyDictionaryItem DictionaryValue { get; set; }
        }
    }
}
