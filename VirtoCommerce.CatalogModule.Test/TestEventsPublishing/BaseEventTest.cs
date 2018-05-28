using System;
using System.Threading;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test.TestEventsPublishing
{
    [Trait("Category", "CI")]
    public class BaseEventTest
    {

        protected virtual Mock<IEventPublisher> GetMockedEventPublisher()
        {
            return new Mock<IEventPublisher>();
        }

        protected virtual void AssignChangedEntriesToLicalVariable<TEvent, TEntity>(Mock<IEventPublisher> mockedEventPublisher, Action<TEvent, CancellationToken> callback) where TEvent : GenericChangedEntryEvent<TEntity>
        {
            mockedEventPublisher
                .Setup(e => e.Publish(It.IsAny<TEvent>(), It.IsAny<CancellationToken>()))
                .Callback(callback);
        }

        protected virtual AbstractValidator<IHasProperties> GetValidator()
        {
            var validationResult = GetMockedValidationResult();
            validationResult.Setup(r => r.IsValid).Returns(true);

            var validator = new Mock<AbstractValidator<IHasProperties>>();
            validator.Setup(v => v.Validate(It.IsAny<ValidationContext<IHasProperties>>())).Returns(validationResult.Object);

            return validator.Object;
        }

        private Mock<ValidationResult> GetMockedValidationResult()
        {
            return new Mock<ValidationResult>();
        }

        protected virtual Mock<ICatalogRepository> GetMockedCatalogRepository()
        {
            var mockedRepo = new Mock<ICatalogRepository>();
            mockedRepo.Setup(r => r.UnitOfWork).Returns(GetUnitOfWork);

            return mockedRepo;
        }

        protected virtual IUnitOfWork GetUnitOfWork()
        {
            return new Mock<IUnitOfWork>().Object;
        }
    }
}
