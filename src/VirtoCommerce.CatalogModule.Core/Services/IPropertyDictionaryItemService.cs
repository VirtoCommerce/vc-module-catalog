using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    /// <summary>
    /// The abstraction represent the CRUD operations for property dictionary items
    /// </summary>
    public interface IPropertyDictionaryItemService : ICrudService<PropertyDictionaryItem>
    {
        [Obsolete("Use GetAsync(IList<string> ids)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        Task<PropertyDictionaryItem[]> GetByIdsAsync(string[] ids);

        [Obsolete("Use SaveChangesAsync(IList<PropertyDictionaryItem> models)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        Task SaveChangesAsync(PropertyDictionaryItem[] dictItems);

        [Obsolete("Use DeleteAsync(IList<string> ids, bool softDelete)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        Task DeleteAsync(string[] ids);
    }
}
