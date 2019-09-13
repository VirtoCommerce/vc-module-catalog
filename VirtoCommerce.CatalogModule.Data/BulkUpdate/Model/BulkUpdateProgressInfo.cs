using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model
{
    public class BulkUpdateProgressInfo
    {
        public BulkUpdateProgressInfo()
        {
            Errors = new List<string>();
        }

        public string Description { get; set; }
        public List<string> Errors { get; set; }
        public int? ProcessedCount { get; set; }
        public int? TotalCount { get; set; }
    }
}
