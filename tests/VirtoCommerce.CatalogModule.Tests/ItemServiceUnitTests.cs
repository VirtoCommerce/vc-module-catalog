using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ItemServiceUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICatalogRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<AbstractValidator<IHasProperties>> _hasPropertyValidatorMock;
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IOutlineService> _outlineServiceMock;
        private readonly Mock<IBlobUrlResolver> _blobUrlResolverMock;
        private readonly Mock<ISkuGenerator> _skuGeneratorMock;
        private readonly Mock<IPropertyValueSanitizer> _propertyValueSanitizerMock;

        public ItemServiceUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<ICatalogRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _hasPropertyValidatorMock = new Mock<AbstractValidator<IHasProperties>>();
            _catalogServiceMock = new Mock<ICatalogService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _outlineServiceMock = new Mock<IOutlineService>();
            _blobUrlResolverMock = new Mock<IBlobUrlResolver>();
            _skuGeneratorMock = new Mock<ISkuGenerator>();
            _propertyValueSanitizerMock = new Mock<IPropertyValueSanitizer>();
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveItem_ReturnCachedItem()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newItem = new CatalogProduct
            {
                Id = id,
                CatalogId = Guid.NewGuid().ToString(),
                CategoryId = Guid.NewGuid().ToString(),
                Name = "some product",
                Code = "some code"
            };
            var newItemEntity = AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(newItem, new PrimaryKeyResolvingMap());
            var service = GetItemServiceWithPlatformMemoryCache();
            _repositoryMock.Setup(x => x.Add(newItemEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetItemByIdsAsync(new[] { id }, null))
                        .ReturnsAsync(new[] { newItemEntity });
                });

            _catalogServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
                .ReturnsAsync(new[] { new Catalog { Id = newItem.CatalogId } });

            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
                .ReturnsAsync(new[] { new Category { Id = newItem.CategoryId } });

            //Act
            var nullItem = await service.GetByIdAsync(id);
            await service.SaveChangesAsync(new[] { newItem });
            var item = await service.GetByIdAsync(id);

            //Assert
            Assert.NotEqual(nullItem, item);
        }

        private ItemService GetItemServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetItemService(platformMemoryCache, _repositoryMock.Object);
        }

        private ItemService GetItemService(IPlatformMemoryCache platformMemoryCache, ICatalogRepository catalogRepository)
        {
            _hasPropertyValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), default))
                .ReturnsAsync(new ValidationResult());

            return new ItemService(() => catalogRepository,
                platformMemoryCache,
                _eventPublisherMock.Object,
                _hasPropertyValidatorMock.Object,
                _catalogServiceMock.Object,
                _categoryServiceMock.Object,
                _outlineServiceMock.Object,
                _blobUrlResolverMock.Object,
                _skuGeneratorMock.Object,
                new ProductValidator(new PropertyValidator()),
                _propertyValueSanitizerMock.Object);
        }
    }
}
