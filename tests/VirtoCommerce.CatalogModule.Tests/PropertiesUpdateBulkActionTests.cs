using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Actions.PropertiesUpdate;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.BulkActions.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class PropertiesUpdateBulkActionTests
    {
        private readonly Mock<IProductService> _productServiceMock;

        public PropertiesUpdateBulkActionTests()
        {
            _productServiceMock = new Mock<IProductService>();
        }

        [Fact]
        public void Context_Result_NotBeNull()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var result = bulkAction.Context;

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Execute_BulkPropertyUpdateManager_InvokeUpdateProperties()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var manager = new Mock<IBulkPropertyUpdateManager>();
            context.Properties = new Property[] { };
            var bulkAction = BuildBulkAction(context, manager);

            // act
            await bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());

            // assert
            manager.Verify(t => t.UpdatePropertiesAsync(It.IsAny<CatalogProduct[]>(), context.Properties));
        }

        [Fact]
        public async Task Execute_ProductService_InvokeGetByIds()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var productService = new Mock<IProductService> { DefaultValueProvider = DefaultValueProvider.Mock };
            context.Properties = new Property[] { };
            var bulkAction = BuildBulkAction(context, productService);

            // act
            await bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());

            // assert
            productService.Verify(
                t => t.GetAsync(
                    It.IsAny<IList<string>>(),
                    (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString(),
                    It.IsAny<bool>()));
        }

        [Fact]
        public void Execute_ShouldResolve_IBulkPropertyUpdateManager()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            try
            {
                bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());
            }
            catch
            {
                // idle
            }

            // assert
            //TODO
        }

        [Fact]
        public Task Execute_UnknownTypeOfListEntry_ArgumentException()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var action = async () =>
                {
                    await bulkAction
                        .ExecuteAsync(new List<IEntity> { new ListEntryBase { Type = "somType" } });
                };

            // assert
            return action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetActionData_BulkPropertyUpdateManager_InvokeGetProperties()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var manager = new Mock<IBulkPropertyUpdateManager>();
            var bulkAction = BuildBulkAction(context, manager);

            // act
            await bulkAction.GetActionDataAsync();

            // assert
            manager.Verify(t => t.GetPropertiesAsync(context));
        }

        [Fact]
        public async Task GetActionData_Result_NotBeNull()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.GetActionDataAsync();

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Validate_Result_True()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var result = await bulkAction.ValidateAsync();

            // assert
            result.Succeeded.Should().Be(true);
        }

        private IBulkAction BuildBulkAction(PropertiesUpdateBulkActionContext context)
        {
            var manager = new Mock<IBulkPropertyUpdateManager>();
            manager.Setup(t => t.GetPropertiesAsync(It.IsAny<PropertiesUpdateBulkActionContext>()))
                .ReturnsAsync(new List<Property>().ToArray());

            var bulkAction = BuildBulkAction(context, manager);

            return bulkAction;
        }

        private IBulkAction BuildBulkAction(
            PropertiesUpdateBulkActionContext context,
            IMock<IBulkPropertyUpdateManager> manager)
        {
            return BuildBulkAction(context, _productServiceMock, manager);
        }

        private IBulkAction BuildBulkAction(
            PropertiesUpdateBulkActionContext context,
            IMock<IProductService> productServiceMock)
        {
            var manager = new Mock<IBulkPropertyUpdateManager>();
            manager.Setup(t => t.GetPropertiesAsync(It.IsAny<PropertiesUpdateBulkActionContext>()))
                .ReturnsAsync(new List<Property>().ToArray());

            return BuildBulkAction(context, productServiceMock, manager);
        }

        private IBulkAction BuildBulkAction(
            PropertiesUpdateBulkActionContext context,
            IMock<IProductService> productServiceMock,
            IMock<IBulkPropertyUpdateManager> manager)
        {
            return new PropertiesUpdateBulkAction(context, productServiceMock.Object, manager.Object);
        }
    }
}
