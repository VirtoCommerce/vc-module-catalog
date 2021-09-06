using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="ICrudService<PropertyDictionaryItem>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IPropertyDictionaryItemService
    {
        [Obsolete(@"Need to remove after inherit IPropertyDictionaryItemService from ICrudService<PropertyDictionaryItem>")]
        Task<PropertyDictionaryItem[]> GetByIdsAsync(string[] ids);
        [Obsolete(@"Need to remove after inherit IPropertyDictionaryItemService from ICrudService<PropertyDictionaryItem>")]
        Task SaveChangesAsync(PropertyDictionaryItem[] dictItems);
        [Obsolete(@"Need to remove after inherit IPropertyDictionaryItemService from ICrudService<PropertyDictionaryItem>")]
        Task DeleteAsync(string[] ids);
    }
}
