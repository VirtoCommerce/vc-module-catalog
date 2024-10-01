using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable;
using Moq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class ProductDocumentChangesProvideTests
    {
        private const int TAKE = 10;
        private const int DAYS_OF_WEEK = 7;
        private readonly Fixture _fixture = new Fixture();

        public ProductDocumentChangesProvideTests()
        {
            _fixture.Register<ItemEntity>(() => _fixture.Build<ItemEntity>()
                .With(x => x.ParentId, () => new Random().Next(0, 1) > 0 ? null : "not null")
                .Without(x => x.Catalog)
                .Without(x => x.Category)
                .Without(x => x.Parent)
                .Without(x => x.CategoryLinks)
                .Without(x => x.Assets)
                .Without(x => x.Images)
                .Without(x => x.Associations)
                .Without(x => x.ReferencedAssociations)
                .Without(x => x.EditorialReviews)
                .Without(x => x.ItemPropertyValues)
                .Without(x => x.SeoInfos)
                .Without(x => x.Childrens)
                .With(i => i.ModifiedDate, () => DateTime.UtcNow.AddDays(new Random().Next(DAYS_OF_WEEK * 3)))
                .Create());
        }

        [Fact]
        public async Task GetTotalChangesCountAsync_DatesNulls_ChangesCountAsExpected()
        {
            // Arrange

            var items = _fixture.CreateMany<ItemEntity>(100);
            var quribleItems = items.AsQueryable().BuildMock();
            var countItemsWithoutParent = items.Count(x => x.ParentId == null);

            var catalogRepositoryMock = new Mock<ICatalogRepository>();
            var changeLogSearchServiceMock = new Mock<IChangeLogSearchService>();

            catalogRepositoryMock.Setup(x => x.Items).Returns(quribleItems);

            var productDocumentChangesProvider = new ProductDocumentChangesProvider(() => catalogRepositoryMock.Object, changeLogSearchServiceMock.Object);

            // Act
            var totalCountOfchanges = await productDocumentChangesProvider.GetTotalChangesCountAsync(null, null);

            // Assert
            catalogRepositoryMock.VerifyAll();
            changeLogSearchServiceMock.VerifyAll();
            Assert.Equal(countItemsWithoutParent, totalCountOfchanges);
        }

        public static IEnumerable<object[]> GetTestStartEndDatesForGetTotalChangesCountAsyncTest()
        {
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2) };
            yield return new object[] { null, DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2) };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), null };
        }

        [Theory]
        [MemberData(nameof(GetTestStartEndDatesForGetTotalChangesCountAsyncTest))]
        public async Task GetTotalChangesCountAsync_DatesNotNull_ChangesCountAsExpected(DateTime? startDate, DateTime? endDate)
        {
            // Arrange

            var items = _fixture.CreateMany<ItemEntity>(150);

            var quribleItems = items.AsQueryable().BuildMock();
            var modifiedItemsCount = items.Count(i => i.ParentId == null
                && (startDate == null || i.ModifiedDate >= startDate)
                && (endDate == null || i.ModifiedDate <= endDate));

            var availableStates = new EntryState[] { EntryState.Added, EntryState.Modified, EntryState.Deleted };

            var operations = _fixture.Build<OperationLog>()
                .With(x => x.OperationType, () => availableStates[new Random().Next(0, availableStates.Length - 1)])
                .CreateMany<OperationLog>(150)
                .ToList();

            var deleteOperations = operations.Where(x => x.OperationType == EntryState.Deleted).ToList();
            var deleteOperationsCount = deleteOperations.Count;

            var catalogRepositoryMock = new Mock<ICatalogRepository>();
            var changeLogSearchServiceMock = new Mock<IChangeLogSearchService>();

            catalogRepositoryMock.Setup(x => x.Items).Returns(quribleItems);

            changeLogSearchServiceMock.Setup(x => x.SearchAsync(new ChangeLogSearchCriteria()
            {
                ObjectType = ProductDocumentChangesProvider.ChangeLogObjectType,
                OperationTypes = new[] { EntryState.Deleted },
                StartDate = startDate,
                EndDate = endDate,
                Take = 0
            })).ReturnsAsync(new ChangeLogSearchResult() { Results = deleteOperations, TotalCount = deleteOperationsCount });

            var productDocumentChangesProvider = new ProductDocumentChangesProvider(() => catalogRepositoryMock.Object, changeLogSearchServiceMock.Object);

            // Act
            var totalCountOfchanges = await productDocumentChangesProvider.GetTotalChangesCountAsync(startDate, endDate);

            // Assert
            var expectedCount = modifiedItemsCount + deleteOperationsCount;

            catalogRepositoryMock.VerifyAll();
            changeLogSearchServiceMock.VerifyAll();
            Assert.Equal(expectedCount, totalCountOfchanges);
        }

        [Theory]
        [InlineData(0, TAKE)]
        [InlineData(TAKE, TAKE)]
        [InlineData(TAKE * 2, TAKE)]
        [InlineData(TAKE * 3, TAKE)]
        [InlineData(TAKE * 4, TAKE)]
        public async Task GetChangesAsync_DatesNulls_ChangesCountAsExpected(int skip, int take)
        {
            // Arrange
            var itemsCountForGenerate = TAKE * 5;

            var items = _fixture.CreateMany<ItemEntity>(itemsCountForGenerate);

            var quribleItems = items.AsQueryable().BuildMock();
            var countItemsWithoutParent = items.Count(x => x.ParentId == null);

            var expectedCount = Math.Min(take, Math.Max(0, countItemsWithoutParent - skip));

            var catalogRepositoryMock = new Mock<ICatalogRepository>();
            var changeLogSearchServiceMock = new Mock<IChangeLogSearchService>();

            catalogRepositoryMock.Setup(x => x.Items).Returns(quribleItems);

            var productDocumentChangesProvider = new ProductDocumentChangesProvider(() => catalogRepositoryMock.Object, changeLogSearchServiceMock.Object);

            // Act
            var indexDocumentChanges = await productDocumentChangesProvider.GetChangesAsync(null, null, skip, take);

            // Assert
            catalogRepositoryMock.VerifyAll();
            changeLogSearchServiceMock.VerifyAll();
            Assert.Equal(expectedCount, indexDocumentChanges.Count);
        }

        public static IEnumerable<object[]> GetTestStartEndDatesForGetChangesAsyncTest()
        {
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), 0, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE * 2, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE * 3, TAKE };
            yield return new object[] { null, DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), 0, TAKE };
            yield return new object[] { null, DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE, TAKE };
            yield return new object[] { null, DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE * 2, TAKE };
            yield return new object[] { null, DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), TAKE * 3, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), null, 0, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK * 2), null, TAKE, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), null, TAKE * 2, TAKE };
            yield return new object[] { DateTime.UtcNow.AddDays(DAYS_OF_WEEK), null, TAKE * 3, TAKE };
        }

        [Theory]
        [MemberData(nameof(GetTestStartEndDatesForGetChangesAsyncTest))]
        public async Task GetChangesAsync_DatesNotNulls_ChangesCountAsExpected(DateTime? startDate, DateTime? endDate, int skip, int take)
        {
            // Arrange
            var itemsCountForGenerate = TAKE * 10;

            var items = _fixture.CreateMany<ItemEntity>(itemsCountForGenerate);

            var quribleItems = items.AsQueryable().BuildMock();
            var modifiedItemsCount = items.Count(i => i.ParentId == null
                && (startDate == null || i.ModifiedDate >= startDate)
                && (endDate == null || i.ModifiedDate <= endDate));

            var availableStates = new EntryState[] { EntryState.Added, EntryState.Modified, EntryState.Deleted };

            var operations = _fixture.Build<OperationLog>()
                .With(x => x.OperationType, () => availableStates[new Random().Next(0, availableStates.Length - 1)])
                .CreateMany<OperationLog>(new Random().Next(take * 5))
                .ToList();

            var deleteOperations = operations.Where(x => x.OperationType == EntryState.Deleted).ToList();
            var deleteOperationsCount = deleteOperations.Count;

            var catalogRepositoryMock = new Mock<ICatalogRepository>();
            var changeLogSearchServiceMock = new Mock<IChangeLogSearchService>();

            catalogRepositoryMock.Setup(x => x.Items).Returns(quribleItems);

            changeLogSearchServiceMock.Setup(x => x.SearchAsync(new ChangeLogSearchCriteria()
            {
                ObjectType = ProductDocumentChangesProvider.ChangeLogObjectType,
                OperationTypes = new[] { EntryState.Deleted },
                StartDate = startDate,
                EndDate = endDate,
                Skip = skip,
                Take = take
            })).ReturnsAsync(new ChangeLogSearchResult() { Results = deleteOperations.Skip(skip).Take(take).ToArray(), TotalCount = deleteOperationsCount });

            var productDocumentChangesProvider = new ProductDocumentChangesProvider(() => catalogRepositoryMock.Object, changeLogSearchServiceMock.Object);

            var totalModifiedNDeletedCount = modifiedItemsCount + deleteOperationsCount;

            var expectedCount = Math.Min(take, Math.Max(0, totalModifiedNDeletedCount - skip));

            // Act

            var indexDocumentChanges = await productDocumentChangesProvider.GetChangesAsync(startDate, endDate, skip, take);

            // Assert
            catalogRepositoryMock.VerifyAll();
            changeLogSearchServiceMock.VerifyAll();
            Assert.Equal(expectedCount, indexDocumentChanges.Count);
        }
    }
}
