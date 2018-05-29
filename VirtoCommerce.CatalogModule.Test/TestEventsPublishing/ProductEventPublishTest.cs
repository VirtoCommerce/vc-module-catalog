using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Moq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;
using Catalog = VirtoCommerce.Domain.Catalog.Model.Catalog;
using Category = VirtoCommerce.Domain.Catalog.Model.Category;
using Property = VirtoCommerce.Domain.Catalog.Model.Property;

namespace VirtoCommerce.CatalogModule.Test.TestEventsPublishing
{
    public class ProductEventPublishTest : BaseEventTest
    {

        [Fact]
        public void TestCreateProductEntityEvent()
        {
            var catalogProduct = GetCatalogProduct();

            var catalogRepo = GetMockedCatalogRepository();

            var commerceService = GetMockedCommerceService();

            var outlineService = GetMockedOutlineService();

            var catalogService = GetMockedCatalogService();
            catalogService.Setup(c => c.GetCatalogsList()).Returns(new[] {GetCatalog()});

            var categoryService = GetMockedCategoryService();

            var eventPublisher = GetMockedEventPublisher();
            var changingEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangingEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangedEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });


            var itemService = GetItemService(catalogRepo.Object, commerceService.Object, outlineService.Object,
                catalogService.Object, categoryService.Object, GetValidator(), eventPublisher.Object);

            itemService.Create(catalogProduct);

            AssertValues<ProductChangingEvent, CatalogProduct>(eventPublisher, changingEventChangedEntries, EntryState.Added);
            AssertValues<ProductChangedEvent, CatalogProduct>(eventPublisher, changedEventChangedEntries, EntryState.Added);
        }

        [Fact]
        public void TestUpdateCatalogEntityEvent()
        {
            var catalogProduct = GetCatalogProduct();

            var catalogRepo = GetMockedCatalogRepository();
            catalogRepo
                .Setup(c => c.GetItemByIds(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>()))
                .Returns(new[] {GetItemEntity()});

            var commerceService = GetMockedCommerceService();

            var outlineService = GetMockedOutlineService();

            var catalogService = GetMockedCatalogService();
            catalogService.Setup(c => c.GetCatalogsList()).Returns(new[] { GetCatalog() });

            var categoryService = GetMockedCategoryService();

            var eventPublisher = GetMockedEventPublisher();
            var changingEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangingEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangedEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var itemService = GetItemService(catalogRepo.Object, commerceService.Object, outlineService.Object,
                catalogService.Object, categoryService.Object, GetValidator(), eventPublisher.Object);

            itemService.Update(new[] {catalogProduct});

            AssertValues<ProductChangingEvent, CatalogProduct>(eventPublisher, changingEventChangedEntries, EntryState.Modified);
            AssertValues<ProductChangedEvent, CatalogProduct>(eventPublisher, changedEventChangedEntries, EntryState.Modified);
        }

        [Fact]
        public void TestDeleteCatalogEntityEvent()
        {
            var catalogRepo = GetMockedCatalogRepository();
            catalogRepo
                .Setup(c => c.GetItemByIds(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>()))
                .Returns(new[] { GetItemEntity() });

            var commerceService = GetMockedCommerceService();

            var outlineService = GetMockedOutlineService();

            var catalogService = GetMockedCatalogService();
            catalogService.Setup(c => c.GetCatalogsList()).Returns(new[] { GetCatalog() });

            var categoryService = GetMockedCategoryService();
            categoryService
                .Setup(c => c.GetByIds(It.IsAny<string[]>(), It.IsAny<CategoryResponseGroup>(), It.IsAny<string>()))
                .Returns(new[] {GetCategory()});

            var eventPublisher = GetMockedEventPublisher();
            var changingEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangingEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<CatalogProduct>>();
            AssignChangedEntriesToLicalVariable<ProductChangedEvent, CatalogProduct>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var itemService = GetItemService(catalogRepo.Object, commerceService.Object, outlineService.Object,
                catalogService.Object, categoryService.Object, GetValidator(), eventPublisher.Object);

            itemService.Delete(new[] { "testCatalogProductId" });

            AssertValues<ProductChangingEvent, CatalogProduct>(eventPublisher, changingEventChangedEntries, EntryState.Deleted);
            AssertValues<ProductChangedEvent, CatalogProduct>(eventPublisher, changedEventChangedEntries, EntryState.Deleted);
        }

        private IItemService GetItemService(ICatalogRepository catalogRepository, ICommerceService commerceService, IOutlineService outlineService,
            ICatalogService catalogService, ICategoryService categoryService, AbstractValidator<IHasProperties> validator, IEventPublisher eventPublisher)
        {
            return new ItemServiceImpl(() => catalogRepository, commerceService, outlineService, catalogService, categoryService, validator, eventPublisher);
        }

        private Mock<ICommerceService> GetMockedCommerceService()
        {
            return new Mock<ICommerceService>();
        }

        private Mock<IOutlineService> GetMockedOutlineService()
        {
            return new Mock<IOutlineService>();
        }

        private Mock<ICategoryService> GetMockedCategoryService()
        {
            return new Mock<ICategoryService>();
        }

        private CatalogProduct GetCatalogProduct()
        {
            return new CatalogProduct
            {
                Id = "testCatalogProductId",
                CatalogId = "testCatalogId",
                Name = "testproductName",
                Code = "testCode",
                Category = GetCategory()
            };
        }

        private Catalog GetCatalog()
        {
            return new Catalog
            {
                Id = "testCatalogId",
                Name = "testCatalogName"
            };
        }

        private ItemEntity GetItemEntity()
        {
            var product = GetCatalogProduct();
            return new ItemEntity
            {
                Id = product.Id,
                CatalogId = product.CatalogId,
                CategoryId = product.Category.Id
            };
        }

        private Category GetCategory()
        {
            return new Category
            {
                Id = "testCategoryId",
                Properties =  new[]
                {
                    new Property(),
                }
            };
        }
    }
}
