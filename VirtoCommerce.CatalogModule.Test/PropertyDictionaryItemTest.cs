using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using Xunit;


namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "CI")]
    public class PropertyDictionaryItemTest
    {
        [Fact]
        public void Add_New_DictionaryItems_To_Property_And_Then_Choose_DictItem_For_Product()
        {
            var propDictionaryService = new Moq.Mock<IProperyDictionaryItemService>().Object;
            var propDictionarySearchService = new Moq.Mock<IProperyDictionaryItemSearchService>();
            var productService = new Moq.Mock<IItemService>();

            var colorProperty = new Property
            {
                Id = "Color",
                Name = "Color",
                CatalogId = "Electronics",
                Type = PropertyType.Product,
                ValueType = PropertyValueType.ShortText,
                Dictionary = true,
                Multilanguage = true,
                Multivalue = true
            };

            var greenDictItem = new PropertyDictionaryItem
            {
                Alias = "Green",
                PropertyId = colorProperty.Id,
                LocalizedValues = new[]
                   {
                        new PropertyDictionaryItemLocalizedValue { LanguageCode = "en", Value = "Green"  },
                        new PropertyDictionaryItemLocalizedValue { LanguageCode = "de", Value = "grün"  }
                   }
            };
            propDictionarySearchService.Setup(x => x.Search(It.IsAny<PropertyDictionaryItemSearchCriteria>())).Returns(new GenericSearchResult<PropertyDictionaryItem> { TotalCount = 1, Results = new[] { greenDictItem } });
            productService.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<ItemResponseGroup>(), null)).Returns(new CatalogProduct { PropertyValues = new List<PropertyValue>() });
            //Add the new dictionary item to the property
            propDictionaryService.SaveChanges(new[] { greenDictItem });


            var product = productService.Object.GetById("Shoes", ItemResponseGroup.ItemProperties);
            //Find the desired dictionary value from all available
            greenDictItem = propDictionarySearchService.Object.Search(new PropertyDictionaryItemSearchCriteria { PropertyIds = new[] { colorProperty.Id }, SearchPhrase = "Green" }).Results.FirstOrDefault();
            //Choose dictionary item for product property
            product.PropertyValues.Add(new PropertyValue { Alias = greenDictItem.Alias, PropertyId = greenDictItem.PropertyId, ValueId = greenDictItem.Id });
            productService.Object.Update(new[] { product });


        }


    }
}
