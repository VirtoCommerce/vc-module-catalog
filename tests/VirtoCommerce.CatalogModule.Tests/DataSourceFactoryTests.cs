using System;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.DataSources;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DataSourceFactoryTests
    {
        [Fact]
        public void Create_EmptyContext_ThrowArgumentException()
        {
            // Arrange
            var dataSourceFactory = BuildDataSourceFactory();
            var context = new BaseBulkActionContext();

            // Act
            var action = new Action(
                () =>
                {
                    dataSourceFactory.Create(context);
                });

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Create_Result_BaseDataSource()
        {
            // Arrange
            var dataSourceFactory = BuildDataSourceFactory();
            var dataQuery = new Mock<DataQuery> { DefaultValueProvider = DefaultValueProvider.Mock };
            var context = new CategoryChangeBulkActionContext { DataQuery = dataQuery.Object };

            // Act
            var result = dataSourceFactory.Create(context);

            // Assert
            result.Should().BeOfType<BaseDataSource>();
        }

        [Fact]
        public void Create_Result_ProductDataSource()
        {
            // Arrange
            var dataSourceFactory = BuildDataSourceFactory();
            var dataQuery = new Mock<DataQuery> { DefaultValueProvider = DefaultValueProvider.Mock };
            var context = new PropertiesUpdateBulkActionContext { DataQuery = dataQuery.Object, Properties = [new Property()] };

            // Act
            var result = dataSourceFactory.Create(context);

            // Assert
            result.Should().BeOfType<ProductDataSource>();
        }

        [Fact]
        public void Create_Result_PropertyDataSource()
        {
            // Arrange
            var dataSourceFactory = BuildDataSourceFactory();
            var dataQuery = new Mock<DataQuery> { DefaultValueProvider = DefaultValueProvider.Mock };
            var context = new PropertiesUpdateBulkActionContext { DataQuery = dataQuery.Object };

            // Act
            var result = dataSourceFactory.Create(context);

            // Assert
            result.Should().BeOfType<PropertyDataSource>();
        }

        private static IDataSourceFactory BuildDataSourceFactory()
        {
            var searchService = new Mock<IInternalListEntrySearchService>();
            var repositoryFactory = new Mock<Func<ICatalogRepository>>();
            var categoryServiceMock = new Mock<ICategoryService>();
            var propertyServiceMock = new Mock<IPropertyService>();
            var itemServiceMock = new Mock<IItemService>();

            return new DataSourceFactory(searchService.Object, repositoryFactory.Object, categoryServiceMock.Object, propertyServiceMock.Object, itemServiceMock.Object);
        }
    }
}
