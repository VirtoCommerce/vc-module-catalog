using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{

    public class PropertyValueEntity : AuditableEntity
    {
        [StringLength(64)]
        public string Alias { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(128)]
        public string KeyValue { get; set; }

        [Required]
        public int ValueType { get; set; }

        [StringLength(512)]
        public string ShortTextValue { get; set; }

        public string LongTextValue { get; set; }

        public decimal DecimalValue { get; set; }

        public int IntegerValue { get; set; }

        public bool BooleanValue { get; set; }

        public DateTime? DateTimeValue { get; set; }

        [StringLength(64)]
        public string Locale { get; set; }

        [NotMapped]
        public object Value
        {
            get
            {
                switch (this.ValueType)
                {
                    case (int)PropertyValueType.Boolean:
                        return this.BooleanValue;
                    case (int)PropertyValueType.DateTime:
                        return this.DateTimeValue;
                    case (int)PropertyValueType.Number:
                        return this.DecimalValue;
                    case (int)PropertyValueType.LongText:
                        return this.LongTextValue;
                    case (int)PropertyValueType.Integer:
                        return this.IntegerValue;
                    default:
                        return this.ShortTextValue;
                }

            }
            set
            {
                switch ((PropertyValueType)this.ValueType)
                {
                    case PropertyValueType.LongText:
                        this.LongTextValue = Convert.ToString(value);
                        break;
                    case PropertyValueType.ShortText:
                        this.ShortTextValue = Convert.ToString(value);
                        break;
                    case PropertyValueType.Number:
                        this.DecimalValue = Convert.ToDecimal(value);                     
                        break;
                    case PropertyValueType.DateTime:
                        this.DateTimeValue = Convert.ToDateTime(value);
                        break;
                    case PropertyValueType.Boolean:
                        this.BooleanValue = Convert.ToBoolean(value);
                        break;
                    case PropertyValueType.Integer:
                        this.IntegerValue = Convert.ToInt32(value);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        #region Navigation Properties
        public string ItemId { get; set; }
        public virtual ItemEntity CatalogItem { get; set; }

        public string CatalogId { get; set; }
        public virtual CatalogEntity Catalog { get; set; }

        public string CategoryId { get; set; }
        public virtual CategoryEntity Category { get; set; }
        #endregion

        public virtual PropertyValue ToModel(PropertyValue propValue)
        {
            if (propValue == null)
                throw new ArgumentNullException(nameof(propValue));

            propValue.Id = this.Id;
            propValue.CreatedBy = this.CreatedBy;
            propValue.CreatedDate = this.CreatedDate;
            propValue.ModifiedBy = this.ModifiedBy;
            propValue.ModifiedDate = this.ModifiedDate;

            propValue.Alias = this.Alias;
            propValue.LanguageCode = this.Locale;
            propValue.PropertyName = this.Name;
            propValue.Value = this.Value;
            propValue.ValueId = this.KeyValue;
            propValue.ValueType = (PropertyValueType)this.ValueType;

            return propValue;
        }

        public virtual PropertyValueEntity FromModel(PropertyValue propValue, PrimaryKeyResolvingMap pkMap)
        {
            if (propValue == null)
                throw new ArgumentNullException(nameof(propValue));

            pkMap.AddPair(propValue, this);

            this.Id = propValue.Id;
            this.CreatedBy = propValue.CreatedBy;
            this.CreatedDate = propValue.CreatedDate;
            this.ModifiedBy = propValue.ModifiedBy;
            this.ModifiedDate = propValue.ModifiedDate;

            this.Alias = propValue.Alias;
            this.Locale = propValue.LanguageCode;
            this.Name = propValue.PropertyName;
            this.Value = propValue.Value;
            this.KeyValue = propValue.ValueId;
            this.ValueType = (int)propValue.ValueType;

            return this;
        }

        public virtual void Patch(PropertyValueEntity target)
        {
            target.Alias = this.Alias;
            target.BooleanValue = this.BooleanValue;
            target.DateTimeValue = this.DateTimeValue;
            target.DecimalValue = this.DecimalValue;
            target.IntegerValue = this.IntegerValue;
            target.KeyValue = this.KeyValue;
            target.Locale = this.Locale;
            target.LongTextValue = this.LongTextValue;
            target.Name = this.Name;
            target.ShortTextValue = this.ShortTextValue;
            target.ValueType = this.ValueType;
        }     

       
    }
}
