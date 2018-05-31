using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CacheManager.Core;
using FluentValidation;
using FluentValidation.Results;
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
    public class CatalogEventsPublishTest : BaseEventTest
    {

        [Fact]
        public void UpdateCatalog_ChecksThatDomainEventsFired()
        {
            //Arrange
            var catalog = new Catalog
            {
                Id = "catalog",
                Languages = new[] { new CatalogLanguage { LanguageCode = "en-US", IsDefault = true } }
            };
            var catalogEntity = new CatalogEntity().FromModel(catalog, new PrimaryKeyResolvingMap());

            var catalogRepositoryFactory = new Mock<ICatalogRepository>();
            catalogRepositoryFactory.Setup(x => x.GetCatalogsByIds(It.IsAny<string[]>())).Returns(new[] { catalogEntity });
            catalogRepositoryFactory.Setup(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            var cacheManager = new Mock<ICacheManager<object>>();
            var eventPublisher = new Mock<IEventPublisher>();
            var validator = new Mock<AbstractValidator<IHasProperties>>();
            validator.Setup(v => v.Validate(It.IsAny<ValidationContext<IHasProperties>>())).Returns(new ValidationResult() { });

            var catalogService = new CatalogServiceImpl(() => catalogRepositoryFactory.Object, cacheManager.Object, validator.Object, eventPublisher.Object);

            //Act
            catalogService.Update(new[] { catalog });

            //Assertion
            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

        }

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

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => GetMockedCatalogRepository().Object, GetMockedCacheManager().Object);

            catalogService.Create(catalog);

            AssertValues<CatalogChangingEvent, Catalog>(eventPublisher, changingEventChangedEntries, EntryState.Added);
            AssertValues<CatalogChangedEvent, Catalog>(eventPublisher, changedEventChangedEntries, EntryState.Added);
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

            var mockedRepo = GetMockedCatalogRepository();
            mockedRepo.Setup(r => r.GetCatalogsByIds(It.IsAny<string[]>())).Returns(new[] { catalogEntity });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => mockedRepo.Object, GetMockedCacheManager().Object);

            catalogService.Update(new[] { catalog });

            AssertValues<CatalogChangingEvent, Catalog>(eventPublisher, changingEventChangedEntries, EntryState.Modified);
            AssertValues<CatalogChangedEvent, Catalog>(eventPublisher, changedEventChangedEntries, EntryState.Modified);
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

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => GetMockedCatalogRepository().Object, cacheManager.Object);

            catalogService.Delete(new[] { "testCatalogId" });

            AssertValues<CatalogChangingEvent, Catalog>(eventPublisher, changingEventChangedEntries, EntryState.Deleted);
            AssertValues<CatalogChangedEvent, Catalog>(eventPublisher, changedEventChangedEntries, EntryState.Deleted);
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
