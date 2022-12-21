using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule2.Core.Model.Search
{
    public class CategoryIndexedSearchCriteria2 : CategoryIndexedSearchCriteria
    {
        public override CatalogIndexedSearchCriteria FromListEntryCriteria(CatalogListEntrySearchCriteria listEntryCriteria)
        {
            return base.FromListEntryCriteria(listEntryCriteria);
        }
    }
}
