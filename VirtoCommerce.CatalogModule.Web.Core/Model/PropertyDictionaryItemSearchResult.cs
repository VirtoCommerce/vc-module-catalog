using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Model
{
	public class PropertyDictionaryItemSearchResult
    {
        public PropertyDictionaryItemSearchResult()
        {
            PropertyDictionaryItems = new List<PropertyDictionaryItem>();
        }

		public int TotalCount { get; set; }

		public ICollection<PropertyDictionaryItem> PropertyDictionaryItems { get; set; }
    }
}
