using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CacheManager.Core;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Omu.ValueInjecter;
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
            var catalog = new Catalog
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

            var validationResult = GetMockedValidationResult();
            validationResult.Setup(r => r.IsValid).Returns(true);

            var validator = GetMockedAbstractValidator();
            validator.Setup(v => v.Validate(It.IsAny<ValidationContext<IHasProperties>>())).Returns(validationResult.Object);

            var changedEntries = new List<GenericChangedEntry<Catalog>>();

            var eventPublisher = GetMockedEventPublisher();
            eventPublisher
                .Setup(e => e.Publish(It.IsAny<CatalogChangingEvent>(), It.IsAny<CancellationToken>()))
                .Callback<CatalogChangingEvent, CancellationToken>((changingEvent, token) =>
                {
                    changedEntries = changingEvent.ChangedEntries.ToList();
                });

            var catalogService = GetCatalogService(validator.Object, eventPublisher.Object);

            catalogService.Create(catalog);

            Assert.Equal(EntryState.Added, changedEntries.Single().EntryState);
            Assert.IsType<Catalog>(changedEntries.Single().NewEntry);
        }

        [Fact]
        public void TestUpdateCatalogEntityEvent()
        {

        }

        private Mock<ValidationResult> GetMockedValidationResult()
        {
            return new Mock<ValidationResult>();
        }

        private ICatalogRepository GetCatalogRepositoryFactoryMethod()
        {
            var mocekdRepo = new Mock<ICatalogRepository>();
            mocekdRepo.Setup(r => r.UnitOfWork).Returns(GetUnitOfWork);

            return mocekdRepo.Object;
        }

        private Mock<AbstractValidator<IHasProperties>> GetMockedAbstractValidator()
        {
            return new Mock<AbstractValidator<IHasProperties>>();
        }

        private ICatalogService GetCatalogService(AbstractValidator<IHasProperties> validator, IEventPublisher eventPublisher)
        {
            return new CatalogServiceImpl(
                GetCatalogRepositoryFactoryMethod,
                new Mock<ICacheManager<object>>().Object,
                validator,
                eventPublisher
                );
        }
    }
}
