using Moq;
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
    }
}
