using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class VideoCreateRequest : ValueObject
    {
        public string ContentUrl { get; set; }
        public int? SortOrder { get; set; }
        public string LanguageCode { get; set; }
        public string OwnerId { get; set; }
        public string OwnerType { get; set; }
    }
}
