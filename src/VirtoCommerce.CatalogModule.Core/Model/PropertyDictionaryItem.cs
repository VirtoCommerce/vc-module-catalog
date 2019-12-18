using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class PropertyDictionaryItem : Entity, ICloneable, IExportable
    {
        public string PropertyId { get; set; }
        public string Alias { get; set; }
        public int SortOrder { get; set; }
        public ICollection<PropertyDictionaryItemLocalizedValue> LocalizedValues { get; set; }

        #region ICloneable members
        public virtual object Clone()
        {
            var result = MemberwiseClone() as PropertyDictionaryItem;
            result.LocalizedValues = LocalizedValues?.Select(x => x.Clone()).OfType<PropertyDictionaryItemLocalizedValue>().ToList();
            return result;
        }
        #endregion
    }
}
