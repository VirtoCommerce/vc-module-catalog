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
    public class CatalogEventsPublishingTest : BaseEventTest
    {

        [Fact]
        public void TestCreateCatalogEntityEvent()
        {
            var catalog = GetCatalog();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            GetChangedEntires<CatalogChangingEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changingEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var changedEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            GetChangedEntires<CatalogChangedEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changedEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => GetMockedCatalogRepository().Object);

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
            GetChangedEntires<CatalogChangingEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changingEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var changedEventChangedEntries = new List<GenericChangedEntry<Catalog>>();
            GetChangedEntires<CatalogChangedEvent, Catalog>(eventPublisher, (changedEntry, token) =>
                {
                    changedEventChangedEntries = changedEntry.ChangedEntries.ToList();
                });

            var mockedRepo = GetMockedCatalogRepository();
            mockedRepo.Setup(r => r.GetCatalogsByIds(It.IsAny<string[]>())).Returns(new [] { catalogEntity });

            var catalogService = GetCatalogService(GetValidator(), eventPublisher.Object, () => mockedRepo.Object);

            catalogService.Update(new[] { catalog });

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(EntryState.Modified, changingEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changingEventChangedEntries.Single().OldEntry);

            eventPublisher.Verify(e => e.Publish(It.IsAny<CatalogChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.Equal(EntryState.Modified, changedEventChangedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changedEventChangedEntries.Single().OldEntry);
        }

        private AbstractValidator<IHasProperties> GetValidator()
        {
            var validationResult = GetMockedValidationResult();
            validationResult.Setup(r => r.IsValid).Returns(true);

            var validator = new Mock<AbstractValidator<IHasProperties>>();
            validator.Setup(v => v.Validate(It.IsAny<ValidationContext<IHasProperties>>())).Returns(validationResult.Object);

            return validator.Object;
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

        private Mock<ValidationResult> GetMockedValidationResult()
        {
            return new Mock<ValidationResult>();
        }

        private Mock<ICatalogRepository> GetMockedCatalogRepository()
        {
            var mocekdRepo = new Mock<ICatalogRepository>();
            mocekdRepo.Setup(r => r.UnitOfWork).Returns(GetUnitOfWork);

            return mocekdRepo;
        }

        private ICatalogService GetCatalogService(AbstractValidator<IHasProperties> validator, IEventPublisher eventPublisher, Func<ICatalogRepository> catalogRepositoryFactoryMethod)
        {
            return new CatalogServiceImpl(
                catalogRepositoryFactoryMethod,
                new Mock<ICacheManager<object>>().Object,
                validator,
                eventPublisher
                );
        }
    }
}
