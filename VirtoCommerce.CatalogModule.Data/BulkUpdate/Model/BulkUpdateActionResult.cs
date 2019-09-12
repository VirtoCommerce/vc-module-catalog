using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class BulkUpdateActionResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
    }
}
