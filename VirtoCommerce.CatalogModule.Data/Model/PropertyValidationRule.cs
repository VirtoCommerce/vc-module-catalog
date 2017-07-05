using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyValidationRule
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
    }
}
