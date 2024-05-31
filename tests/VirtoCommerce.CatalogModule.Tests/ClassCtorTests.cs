using System;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.BulkActions.Actions.CategoryChange;
using VirtoCommerce.CatalogModule.BulkActions.Actions.PropertiesUpdate;
using VirtoCommerce.CatalogModule.BulkActions.DataSources;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ClassCtorTests
    {
        [Fact]
        public void PropertiesUpdateBulkAction_NullArgs_ThrowArgumentException()
        {
            // arrange

            // act
            var action = new Action(
                () =>
                {
                    new PropertiesUpdateBulkAction(null, (IProductService)null, null);
                });

            // assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BaseDataSource_NullArgs_ThrowArgumentException()
        {
            // arrange

            // act
            var action = new Action(
                () =>
                {
                    new BaseDataSource(Mock.Of<IInternalListEntrySearchService>(), null);
                });

            // assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CategoryChangeBulkAction_NullArgs_ThrowArgumentException()
        {
            // arrange

            // act
            var action = new Action(
                () =>
                {
                    new CategoryChangeBulkAction(null, null, null, null);
                });

            // assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
