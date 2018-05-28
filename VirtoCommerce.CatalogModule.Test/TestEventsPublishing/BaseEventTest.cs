using System;
using System.Threading;
using Moq;
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

        protected virtual IUnitOfWork GetUnitOfWork()
        {
            return new Mock<IUnitOfWork>().Object;
        }

        protected virtual void GetChangedEntires<TEvent, TEntity>(Mock<IEventPublisher> mockedEventPublisher, Action<TEvent, CancellationToken> callback) where TEvent : GenericChangedEntryEvent<TEntity>
        {
            mockedEventPublisher
                .Setup(e => e.Publish(It.IsAny<TEvent>(), It.IsAny<CancellationToken>()))
                .Callback(callback);
        }
    }
}
