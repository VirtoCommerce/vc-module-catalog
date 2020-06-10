using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DynamicAssociationServiceTests
    {
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
        private readonly Mock<IUnitOfWork> _unityOfWorkMock;

        public DynamicAssociationServiceTests()
        {
            _eventPublisherMock = new Mock<IEventPublisher>();
            _catalogRepositoryMock = new Mock<ICatalogRepository>();
            _unityOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveDynamicAssociation()
        {
            var id = Guid.NewGuid().ToString();
            var dynamicAssociation = new DynamicAssociation { Id = id, };
            var dynamicAssociationEntity = AbstractTypeFactory<DynamicAssociationEntity>.TryCreateInstance().FromModel(dynamicAssociation, new PrimaryKeyResolvingMap());

            var dynamicAssociationService = CreateDynamicAssociationService();
            _catalogRepositoryMock.Setup(x => x.Add(dynamicAssociationEntity)).Callback(() =>
            {
                _catalogRepositoryMock
                    .Setup(x => x.GetDynamicAssociationsByIdsAsync(new[] {id}))
                    .ReturnsAsync(new[] {dynamicAssociationEntity});
            });

            var nullDynamicAssociation = await dynamicAssociationService.GetByIdsAsync(new[] { id });
            await dynamicAssociationService.SaveChangesAsync(new[] { dynamicAssociation });
            var dynamicAssociationFromService = await dynamicAssociationService.GetByIdsAsync(new [] { id });

            Assert.NotEqual(nullDynamicAssociation, dynamicAssociationFromService);
        }


        private DynamicAssociationService CreateDynamicAssociationService()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _catalogRepositoryMock.Setup(x => x.UnitOfWork).Returns(_unityOfWorkMock.Object);

            var result = new DynamicAssociationService(() => _catalogRepositoryMock.Object, platformMemoryCache, _eventPublisherMock.Object);

            return result;
        }
    }
}
