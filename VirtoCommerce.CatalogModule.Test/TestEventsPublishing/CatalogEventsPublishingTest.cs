using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CacheManager.Core;
using FluentValidation;
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
    public class CatalogEventsPublishingTest : BaseEventTest
    {

        [Fact]
        public void TestCreateCatalogEntityEvent()
        {
            var catalog = GetCatalog();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangingEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changingEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var changedEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangedEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changedEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => GetMockedRepository<ICatalogRepository>().Object, GetMockedCacheManager().Object);

            catalogService.Create(catalog);

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.Equal(EntryState.Added, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changingEventChangedEntries.Single().NewEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.Equal(EntryState.Added, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changedEventChangedEntries.Single().NewEntry);
        }

        [Fact]
        public void TestUpdateCatalogEntityEvent()
        {
            var catalog = GetCatalog();

            var catalogEntity = new CatalogEntity
            {
                Id = "testCatalogId"
            };

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangingEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changingEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var changedEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangedEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changedEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var mockedRepo = GetMockedRepository<ICatalogRepository>();
            mockedRepo.Setup(r => r.GetCatalogsByIds(It.IsAny<string[]>())).Returns(new [] { catalogEntity });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => mockedRepo.Object, GetMockedCacheManager().Object);

            catalogService.Update(new[] { catalog });

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(EntryState.Modified, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changingEventChangedEntries.Single().OldEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.Equal(EntryState.Modified, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changedEventChangedEntries.Single().OldEntry);
        }

        [Fact]
        public void TestDeleteCatalogEntityEvent()
        {
            var catalog = GetCatalog();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangingEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changingEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var changedEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            AssignChangedEntriesToLicalVariable<CatalogChangedEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changedEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var cacheManager = GetMockedCacheManager();
            cacheManager.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<string, Catalog>
            {
                {"testCatalogId", catalog}
            });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => GetMockedRepository<ICatalogRepository>().Object, cacheManager.Object);

            catalogService.Delete(new [] {"testCatalogId"});

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(EntryState.Deleted, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changingEventChangedEntries.Single().OldEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.Equal(EntryState.Deleted, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changedEventChangedEntries.Single().OldEntry);
        }

        private Mock<ICacheManager<object>> GetMockedCacheManager()
        {
            return new Mock<ICacheManager<object>>();
        }

        private Catalog GetCatalog()
        {
            return new Catalog
            {
                Id = "testCatalogId",
                Languages = new List<CatalogLanguage>
                {
                    new CatalogLanguage
                    {
                        IsDefault = true
                    }
                }
            };
        }

        private ICatalogService GetCatalogService(AbstractValidator<IHasProperties> validator, IEventPublisher eventPublisher, Func<ICatalogRepository> catalogRepositoryFactoryMethod, ICacheManager<object> cacheManager)
        {
            return new CatalogServiceImpl(
                catalogRepositoryFactoryMethod,
                cacheManager,
                validator,
                eventPublisher
                );
        }
    }
}
