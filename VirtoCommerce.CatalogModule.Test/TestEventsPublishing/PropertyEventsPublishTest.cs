using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CacheManager.Core;
using Moq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test.TestEventsPublishing
{
    public class PropertyEventsPublishTest : BaseEventTest
    {
        [Fact]
        public void TestCreatePropertyEvent()
        {
            var property = GetProperty();

            var catalogRepo = GetMockedCatalogRepository();

            var cacheManager = GetMockedCacheManager();

            var catalogService = GetMockedCatalogService();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangingEvent, Property>(eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangedEvent, Property>(eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var propertyService = GetPropertyService(catalogRepo.Object, cacheManager.Object, catalogService.Object,
                eventPublisher.Object);

            propertyService.Create(property);

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangingEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(EntryState.Added, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changingEventChangedEntries.Single().NewEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once());

            Assert.Equal(EntryState.Added, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changedEventChangedEntries.Single().NewEntry);
        }

        [Fact]
        public void TestUpdatePropertyEvent()
        {
            var property = GetProperty();

            var catalogRepo = GetMockedCatalogRepository();
            catalogRepo.Setup(c => c.GetPropertiesByIds(It.IsAny<string[]>())).Returns(new[] {GetPropertyEntity()});

            var cacheManager = GetMockedCacheManager();

            var catalogService = GetMockedCatalogService();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangingEvent, Property>(eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangedEvent, Property>(eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var propertyService = GetPropertyService(catalogRepo.Object, cacheManager.Object, catalogService.Object,
                eventPublisher.Object);

            propertyService.Update(new [] {property});

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangingEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(EntryState.Modified, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changingEventChangedEntries.Single().OldEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once());

            Assert.Equal(EntryState.Modified, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changedEventChangedEntries.Single().OldEntry);
        }

        [Fact]
        public void TestDeletePropertyEvent()
        {
            var property = GetProperty();

            var catalogRepo = GetMockedCatalogRepository();

            var cacheManager = GetMockedCacheManager();
            cacheManager
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    new Dictionary<string, Property>
                    {
                        {"testProperty", property}
                    });

            var catalogService = GetMockedCatalogService();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangingEvent, Property>(eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Property>>();
            AssignChangedEntriesToLicalVariable<PropertyChangedEvent, Property>(eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var propertyService = GetPropertyService(catalogRepo.Object, cacheManager.Object, catalogService.Object,
                eventPublisher.Object);

            propertyService.Delete(new[] {"testProperty"});

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangingEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(EntryState.Deleted, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changingEventChangedEntries.Single().OldEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<PropertyChangedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once());

            Assert.Equal(EntryState.Deleted, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Property>(changedEventChangedEntries.Single().OldEntry);
        }

        private IPropertyService GetPropertyService(ICatalogRepository catalogRepository,
            ICacheManager<object> cacheManager, ICatalogService catalogService, IEventPublisher eventPublisher)
        {
            return new PropertyServiceImpl(() => catalogRepository, cacheManager, catalogService, eventPublisher);
        }

        private Property GetProperty()
        {
            return new Property
            {
                Id = "testProperty",
                CatalogId = "testCatalogId"
            };
        }

        private PropertyEntity GetPropertyEntity()
        {
            return new PropertyEntity
            {
                Id = "testProperty"
            };
        }
    }
}
