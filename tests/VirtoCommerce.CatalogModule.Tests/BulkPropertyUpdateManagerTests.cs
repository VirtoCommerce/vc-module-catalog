using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.BulkActions.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class BulkPropertyUpdateManagerTests
    {
        [Fact]
        public async Task GetProperties_DataSource_InvokeFetch()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var dataSource = new Mock<IDataSource>();
            var dataSourceFactory = new Mock<IDataSourceFactory>();
            var manager = BuildManager(dataSourceFactory);
            dataSourceFactory.Setup(t => t.Create(context)).Returns(dataSource.Object);

            // act
            await manager.GetPropertiesAsync(context);

            // assert
            dataSource.Verify(t => t.FetchAsync());
        }

        [Fact]
        public async Task GetProperties_ProductService_InvokeGetAsync()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var dataSourceFactory = new Mock<IDataSourceFactory>();
            var productService = new Mock<IProductService>();
            var manager = BuildManager(dataSourceFactory, productService);
            var dataSource = new Mock<IDataSource>();
            var productId = "fakeProductId";
            var group = ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties;
            var productIds = new List<string> { productId };
            var properties = new List<Property>();
            var product = Mock.Of<CatalogProduct>(
                t => t.Id == productId && t.Properties == properties,
                MockBehavior.Loose);
            var products = new List<CatalogProduct> { product };
            dataSourceFactory.Setup(t => t.Create(context)).Returns(dataSource.Object);
            dataSource.SetupSequence(t => t.FetchAsync()).ReturnsAsync(true).ReturnsAsync(false);
            dataSource.Setup(t => t.Items).Returns(products);
            productService.Setup(t => t.GetAsync(productIds, group.ToString(), It.IsAny<bool>())).ReturnsAsync(products.ToArray());

            // act
            await manager.GetPropertiesAsync(context);

            // assert
            productService.Verify(t => t.GetAsync(productIds, group.ToString(), It.IsAny<bool>()));
        }

        [Theory]
        [InlineData(1)]
        public async Task GetProperties_Should_HaveCountGreaterThan(int count)
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var dataSource = new Mock<IDataSource>();
            var dataSourceFactory = new Mock<IDataSourceFactory>();
            var manager = BuildManager(dataSourceFactory);
            dataSourceFactory.Setup(t => t.Create(context)).Returns(dataSource.Object);

            // act
            var result = await manager.GetPropertiesAsync(context);

            // assert
            result.Should().HaveCountGreaterThan(count);
        }

        private IBulkPropertyUpdateManager BuildManager()
        {
            var dataSourceFactory = new Mock<IDataSourceFactory>();
            var productService = new Mock<IProductService>();
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var dictService = new Mock<IPropertyDictionaryItemSearchService>();
            var singleProductUpdateManager = new PropertyUpdateManager(dictService.Object);

            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, productService.Object, categoryService.Object, catalogService.Object, singleProductUpdateManager);
            return manager;
        }

        private IBulkPropertyUpdateManager BuildManager(IMock<IDataSourceFactory> dataSourceFactory)
        {
            var productService = new Mock<IProductService>();
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var dictService = new Mock<IPropertyDictionaryItemSearchService>();
            var singleProductUpdateManager = new PropertyUpdateManager(dictService.Object);

            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, productService.Object, categoryService.Object, catalogService.Object, singleProductUpdateManager);
            return manager;
        }

        private IBulkPropertyUpdateManager BuildManager(IMock<IDataSourceFactory> dataSourceFactory, IMock<IProductService> productService)
        {
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var dictService = new Mock<IPropertyDictionaryItemSearchService>();
            var singleProductUpdateManager = new PropertyUpdateManager(dictService.Object);

            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, productService.Object, categoryService.Object, catalogService.Object, singleProductUpdateManager);
            return manager;
        }
    }
}
