using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class PropertyDictionaryItemTest
    {
        [Fact]
        public async Task Add_New_DictionaryItems_To_Property_And_Then_Choose_DictItem_For_Product()
        {
            var propDictionaryService = new Mock<IPropertyDictionaryItemService>().Object;
            var propDictionarySearchService = new Mock<IPropertyDictionaryItemSearchService>();
            var productService = new Mock<IProductService>();

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
            propDictionarySearchService
                .Setup(x => x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), true))
                .Returns(Task.FromResult(new PropertyDictionaryItemSearchResult { TotalCount = 1, Results = new[] { greenDictItem } }));

            productService
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { new CatalogProduct { Properties = new List<Property>() } });

            //Add the new dictionary item to the property
            await propDictionaryService.SaveChangesAsync(new List<PropertyDictionaryItem> { greenDictItem });

            var product = await productService.Object.GetByIdAsync("Shoes", ItemResponseGroup.ItemProperties.ToString());
            //Find the desired dictionary value from all available
            greenDictItem = (await propDictionarySearchService.Object.SearchAsync(new PropertyDictionaryItemSearchCriteria { PropertyIds = new[] { colorProperty.Id }, Keyword = "Green" }, clone: true))
                .Results.FirstOrDefault();
            //Choose dictionary item for product property
            product.Properties.Add(new Property
            {
                Name = colorProperty.Name,
                Type = colorProperty.Type,
                ValueType = colorProperty.ValueType,
                Values = new List<PropertyValue> { new() { Alias = greenDictItem.Alias, PropertyId = greenDictItem.PropertyId, ValueId = greenDictItem.Id } },
            });

            await productService.Object.SaveChangesAsync(new[] { product });
        }
    }
}
