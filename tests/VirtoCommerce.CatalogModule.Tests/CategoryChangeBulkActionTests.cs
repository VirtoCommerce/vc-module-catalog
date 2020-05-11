using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Actions.CategoryChange;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryChangeBulkActionTests
    {
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly Mock<ListEntryMover<Category>> _categoryListEntryMoverMock;
        private readonly Mock<ListEntryMover<CatalogProduct>> _productListEntryMoverMock;

        public CategoryChangeBulkActionTests()
        {
            _categoryListEntryMoverMock = new Mock<ListEntryMover<Category>>();
            _productListEntryMoverMock = new Mock<ListEntryMover<CatalogProduct>>();
            _catalogServiceMock = new Mock<ICatalogService>();
        }

        [Fact]
        public void Context_Result_NotNull()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var result = bulkAction.Context;

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Execute_Result_BulkActionResult()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());

            // assert
            result.Should().BeOfType(typeof(BulkActionResult));
        }

        //TODO
        //[Theory]
        //[ClassData(typeof(MethodsInvocationTestData))]
        //public async Task Execute_Should_InvokeMethods(Action assertAction)
        //{
        //    // arrange
        //    var context = new CategoryChangeBulkActionContext();
        //    var bulkAction = BuildBulkAction(context);

        //    // act
        //    await bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());

        //    // assert
        //    assertAction();
        //}

        [Fact]
        public void Execute_Should_ThrowException()
        {
            // arrange
            var categoryId = "fakeId";
            var entries = new List<ListEntryBase> { new ListEntryBase { Id = categoryId } };
            var context = new CategoryChangeBulkActionContext { CategoryId = categoryId };
            var bulkAction = BuildBulkAction(context);

            // act
            var action = new Action(
                () =>
                {
                    bulkAction.ExecuteAsync(entries).GetAwaiter().GetResult();
                });

            // assert
            action.Should().Throw<Exception>();
        }

        [Fact]
        public async Task GetActionData_Result_NullAsync()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext { CatalogId = "catalog" };
            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.GetActionDataAsync();

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Validate_Result_BulkActionResult()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext { CatalogId = "catalog" };
            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ValidateAsync();

            // assert
            result.Should().BeOfType(typeof(BulkActionResult));
        }

        [Fact]
        public async Task Validate_Result_False()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext { CatalogId = "catalog" };

            _catalogServiceMock.Setup(t => t.GetByIdsAsync(new []{ "catalog" }, It.IsAny<string>()))
                .ReturnsAsync(new [] { new Catalog { IsVirtual = true } });

            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ValidateAsync();

            // assert
            result.Succeeded.Should().Be(false);
        }

        [Theory]
        [InlineData(1)]
        public async Task Validate_Result_HaveErrorCount(int errorCount)
        {
            // arrange
            var context = new CategoryChangeBulkActionContext { CatalogId = "catalog" };

            _catalogServiceMock.Setup(t => t.GetByIdsAsync(new[] { "catalog" }, null))
                .ReturnsAsync(new[] { new Catalog { IsVirtual = true } });

            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ValidateAsync();

            // assert
            result.Errors.Should().HaveCount(errorCount, "Because we can't move in virtual catalog'");
        }

        [Fact]
        public async Task Validate_Result_True()
        {
            // arrange
            var context = new CategoryChangeBulkActionContext { CatalogId = "catalog" };
            var catalogService = new Mock<ICatalogService>();

            catalogService.Setup(t => t.GetByIdsAsync(new[] { "catalog" }, null))
                .ReturnsAsync(new[] { new Catalog { IsVirtual = true } });

            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ValidateAsync();

            // assert
            result.Succeeded.Should().Be(true);
        }

        private IBulkAction BuildBulkAction(CategoryChangeBulkActionContext context)
        {
            return new CategoryChangeBulkAction(context, _catalogServiceMock.Object, _categoryListEntryMoverMock.Object, _productListEntryMoverMock.Object);
        }
    }
}
