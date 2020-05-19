using System;
using System.Collections;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule.Tests
{

    public class MethodsInvocationTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var categoryMock = new Mock<ListEntryMover<Category>>();
            var productMock = new Mock<ListEntryMover<CatalogProduct>>();

            var action1 = new Action(
                () =>
                {
                    categoryMock.Verify(t => t.PrepareMoveAsync(It.IsAny<ListEntriesMoveRequest>()));
                });
            yield return new object[] { action1 };

            var action2 = new Action(
                () =>
                {
                    productMock.Verify(t => t.PrepareMoveAsync(It.IsAny<ListEntriesMoveRequest>()));
                });
            yield return new object[] { action2 };

            var action3 = new Action(
                () =>
                {
                    categoryMock.Verify(t => t.ConfirmMoveAsync(It.IsAny<IEnumerable<Category>>()));
                });
            yield return new object[] { action3 };

            var action4 = new Action(
                () =>
                {
                    productMock.Verify(t => t.ConfirmMoveAsync(It.IsAny<IEnumerable<CatalogProduct>>()));
                });
            yield return new object[] { action4 };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
