using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchRequestBuilder : CatalogSearchRequestBuilder
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Category;
    }
}
