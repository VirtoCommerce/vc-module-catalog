using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class BulkUpdateActionResult
    {
        public BulkUpdateActionResult()
        {
            Errors = new List<string>();
        }

        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }

        public static BulkUpdateActionResult Success => new BulkUpdateActionResult() { Succeeded = true };
    }
}
