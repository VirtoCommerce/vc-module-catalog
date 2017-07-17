using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyValidationRuleEntity : Entity
    {
        public bool IsUnique { get; set; }

        public int? CharCountMin { get; set; }

        public int? CharCountMax { get; set; }

        [StringLength(2048)]
        public string RegExp { get; set; }

        #region Navigation properties

        public string PropertyId { get; set; }

        public PropertyEntity Property { get; set; }

        #endregion


        public virtual PropertyValidationRule ToModel(PropertyValidationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            rule.Id = this.Id;
            rule.CharCountMax = this.CharCountMax;
            rule.CharCountMin = this.CharCountMin;
            rule.IsUnique = this.IsUnique;
            rule.RegExp = this.RegExp;
            rule.PropertyId = this.PropertyId;

            return rule;
        }

        public virtual PropertyValidationRuleEntity FromModel(PropertyValidationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            this.Id = rule.Id;
            this.CharCountMax = rule.CharCountMax;
            this.CharCountMin = rule.CharCountMin;
            this.IsUnique = rule.IsUnique;
            this.RegExp = rule.RegExp;
            this.PropertyId = rule.PropertyId;

            return this;
        }

        public virtual void Patch(PropertyValidationRuleEntity target)
        {
            target.CharCountMax = this.CharCountMax;
            target.CharCountMin = this.CharCountMin;
            target.IsUnique = this.IsUnique;
            target.RegExp = this.RegExp;
        }
    }
}
