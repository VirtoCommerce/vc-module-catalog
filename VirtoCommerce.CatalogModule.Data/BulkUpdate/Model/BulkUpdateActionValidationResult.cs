using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class BulkUpdateActionValidationResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
    }
}
