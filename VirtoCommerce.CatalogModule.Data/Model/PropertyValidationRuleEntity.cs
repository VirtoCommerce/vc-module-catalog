using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


        //public virtual PropertyValidationRule ToModel(PropertyValidationRule displayName)
        //{
        //}

        //public virtual PropertyValidationRuleEntity FromModel(PropertyValidationRule displayName)
        //{
        //}

        public virtual void Patch(PropertyValidationRuleEntity target)
        {

        }
    }
}
