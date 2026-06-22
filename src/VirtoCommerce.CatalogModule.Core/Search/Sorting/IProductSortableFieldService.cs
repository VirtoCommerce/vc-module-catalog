using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Provides the set of fields the admin can pick in a sort clause. The list is derived from the product index
/// schema, so adding a sortable field to the index (in any module) surfaces it automatically.
/// </summary>
public interface IProductSortableFieldService
{
    Task<IList<ProductSortableField>> GetSortableFieldsAsync(string storeId);
}
