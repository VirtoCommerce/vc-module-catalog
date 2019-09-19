using VirtoCommerce.Domain.Catalog.Model;
using web = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public interface IBulkUpdatePropertyManager
    {
        Property[] GetProperties(UpdatePropertiesActionContext context);
        UpdatePropertiesResult UpdateProperties(CatalogProduct[] products, web.Property[] propertiesToSet);
    }
}
