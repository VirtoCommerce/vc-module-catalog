using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CacheManager.Core;
using FluentValidation;
using Moq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.CoreModule.Data.Repositories;
using VirtoCommerce.CoreModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using Xunit;


namespace VirtoCommerce.CatalogModule.Test
{
    public class CatalogPropertyTest
    {
        [Fact]
        public void AddNewPropertiesProgrammably()
        {
            var propertyService = GetPropertyService();

            var newProperty = new Property
            {
                //Electronics
                CatalogId = "4974648a41df4e6ea67ef2ad76d7bbd4",                
                Dictionary = true,
                Name = "color2",
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText
            };
            newProperty.DictionaryValues = new List<PropertyDictionaryValue>();
            var colorDictValue = new PropertyDictionaryValue
            {
                Alias = "green2",
                Value = "green2"
            };
            newProperty.DictionaryValues.Add(colorDictValue);
            newProperty = propertyService.Create(newProperty);

            var productService = GetItemService();
            var product = productService.GetById("93beb4e92dba4a08a173aa0a0cf0cffb", ItemResponseGroup.ItemInfo);
            //Set value 
            product.PropertyValues.Add(new PropertyValue { PropertyId = newProperty.Id, PropertyName = newProperty.Name, ValueId = colorDictValue.Id, Value = colorDictValue.Value, ValueType = newProperty.ValueType });
            productService.Update(new[] { product });

        }
       
        
        private static ICatalogService GetCatalogService()
        {
            return new CatalogServiceImpl(GetCatalogRepository, GetCommerceService(), new Mock<ICacheManager<object>>().Object, new Mock<AbstractValidator<IHasProperties>>().Object);
        }

        private static IItemService GetItemService()
        {
            return null;
           // return new ItemServiceImpl(GetCatalogRepository, GetCommerceService(), new Mock<IOutlineService>().Object, new Mock<ICacheManager<object>>().Object);
        }

        private static IPropertyService GetPropertyService()
        {
            return new PropertyServiceImpl(GetCatalogRepository, new Mock<ICacheManager<object>>().Object, null);
        }

        private static ICommerceService GetCommerceService()
        {
            return new CommerceServiceImpl(() => new CommerceRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null)));
        }

        private static ICatalogRepository GetCatalogRepository()
        {
            var retVal = new CatalogRepositoryImpl(null, "VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null));
            return retVal;
        }

    }
}
