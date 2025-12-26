using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.BulkActions.Services;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class BulkPropertyUpdateManagerTests
    {
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

        private IBulkPropertyUpdateManager BuildManager(IMock<IDataSourceFactory> dataSourceFactory)
        {
            var itemService = new Mock<IItemService>();
            var categoryService = new Mock<ICategoryService>();
            var catalogService = new Mock<ICatalogService>();
            var dictService = new Mock<IPropertyDictionaryItemSearchService>();
            var propertyService = new Mock<IPropertyService>();
            var singleProductUpdateManager = new PropertyUpdateManager(dictService.Object);

            var manager = new BulkPropertyUpdateManager(dataSourceFactory.Object, itemService.Object, categoryService.Object, catalogService.Object, singleProductUpdateManager, propertyService.Object);
            return manager;
        }
    }
}
