using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties;
using VirtoCommerce.Domain.Catalog.Model;
using domainModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public interface IBulkUpdatePropertyManager
    {
        domainModel.Property[] GetProperties(UpdatePropertiesActionContext context);
        void UpdateProperties(CatalogProduct[] products, domainModel.PropertyValue[] propertyValues);
    }
}
