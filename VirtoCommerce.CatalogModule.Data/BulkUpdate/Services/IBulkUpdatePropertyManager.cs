using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Domain.Catalog.Model;
using domainModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public interface IBulkUpdatePropertyManager
    {
        domainModel.Property[] GetProperties(ProductBulkUpdateDataQuery dataQuery);
        void UpdateProperties(CatalogProduct[] products, domainModel.PropertyValue[] propertyValues);
    }
}
