using FluentValidation;
using Moq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

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

            var categoryService = GetMockedCategoryService();

            var eventPublisher = GetMockedEventPublisher();

            var itemService = GetItemService(catalogRepo.Object, commerceService.Object, outlineService.Object,
                catalogService.Object, categoryService.Object, GetValidator(), eventPublisher.Object);

            itemService.Create(catalogProduct);
        }

        [Fact]
        public void TestUpdateCatalogEntityEvent()
        {

        }

        [Fact]
        public void TestDeleteCatalogEntityEvent()
        {

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
            return new CatalogProduct();
        }
    }
}
