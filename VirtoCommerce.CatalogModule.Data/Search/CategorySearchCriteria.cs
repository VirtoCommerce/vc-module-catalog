using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchCriteria : CatalogSearchCriteria
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Category;
        public virtual CategoryResponseGroup ResponseGroup { get; set; }
    }
}
