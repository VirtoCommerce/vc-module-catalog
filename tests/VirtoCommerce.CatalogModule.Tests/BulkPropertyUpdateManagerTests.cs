using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.BulkActions.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
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
        public async Task GetProperties_ItemService_InvokeGetByIds()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var dataSourceFactory = new Mock<IDataSourceFactory>();
            var itemService = new Mock<IItemService>();
            var manager = BuildManager(dataSourceFactory, itemService);
            var dataSource = new Mock<IDataSource>();
            var productId = "fakeProductId";
            var group = ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties;
            var productIds = new[] { productId };
            var properties = new List<Property>();
            var product = Mock.Of<CatalogProduct>(
                t => t.Id == productId && t.Properties == properties,
                MockBehavior.Loose);
            var products = new List<CatalogProduct> { product };
            dataSourceFactory.Setup(t => t.Create(context)).Returns(dataSource.Object);
            dataSource.SetupSequence(t => t.FetchAsync()).ReturnsAsync(true).ReturnsAsync(false);
            dataSource.Setup(t => t.Items).Returns(products);
            itemService.Setup(t => t.GetByIdsAsync(productIds, group.ToString(), null)).ReturnsAsync(products.ToArray());

            // act
            await manager.GetPropertiesAsync(context);

            // assert
            itemService.Verify(t => t.GetByIdsAsync(productIds, group.ToString(), null));
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
            var itemService = new Mock<IItemService>();
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, itemService.Object, categoryService.Object, catalogService.Object);
            return manager;
        }

        private IBulkPropertyUpdateManager BuildManager(IMock<IDataSourceFactory> dataSourceFactory)
        {
            var itemService = new Mock<IItemService>();
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();

            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, itemService.Object, categoryService.Object, catalogService.Object);
            return manager;
        }

        private IBulkPropertyUpdateManager BuildManager(IMock<IDataSourceFactory> dataSourceFactory, IMock<IItemService> itemService)
        {
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, itemService.Object, categoryService.Object, catalogService.Object);
            return manager;
        }
    }
}
