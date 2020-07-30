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
        private readonly Mock<IItemService> _itemServiceMock;

        public PropertiesUpdateBulkActionTests()
        {
            _itemServiceMock = new Mock<IItemService>();
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
        public async Task Execute_ItemService_InvokeGetByIds()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var itemService = new Mock<IItemService> { DefaultValueProvider = DefaultValueProvider.Mock };
            context.Properties = new Property[] { };
            var bulkAction = BuildBulkAction(context, itemService);

            // act
            await bulkAction.ExecuteAsync(Enumerable.Empty<IEntity>());

            // assert
            itemService.Verify(
                t => t.GetByIdsAsync(
                    It.IsAny<string[]>(),
                    (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString(),
                    null));
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
        public void Execute_UnknownTypeOfListEntry_ArgumentException()
        {
            // arrange
            var context = new PropertiesUpdateBulkActionContext();
            var bulkAction = BuildBulkAction(context);

            // act
            var action = new Action(() =>
                {
                    bulkAction
                        .ExecuteAsync(new List<IEntity> { new ListEntryBase { Type = "somType"} })
                        .GetAwaiter()
                        .GetResult();
                });

            // assert
            action.Should().Throw<ArgumentException>();
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
            return BuildBulkAction(context, _itemServiceMock, manager);
        }

        private IBulkAction BuildBulkAction(
            PropertiesUpdateBulkActionContext context,
            IMock<IItemService> itemServiceMock)
        {
            var manager = new Mock<IBulkPropertyUpdateManager>();
            manager.Setup(t => t.GetPropertiesAsync(It.IsAny<PropertiesUpdateBulkActionContext>()))
                .ReturnsAsync(new List<Property>().ToArray());

            return BuildBulkAction(context, itemServiceMock, manager);
        }

        private IBulkAction BuildBulkAction(
            PropertiesUpdateBulkActionContext context,
            IMock<IItemService> itemServiceMock,
            IMock<IBulkPropertyUpdateManager> manager)
        {
            return new PropertiesUpdateBulkAction(context, itemServiceMock.Object, manager.Object);
        }
    }
}
