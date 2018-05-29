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
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test.TestEventsPublishing
{
    public class CategoryEventsPublishTest : BaseEventTest
    {
        [Fact]
        public void TestCreateCategoryEntityEvent()
        {
            var category = GetCategory();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangingEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangedEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var cacheManager = GetMockedCacheManager();
            cacheManager.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<string, Category>
            {
                {"testCategoryId", category}
            });

            var catalogService = GetMockedCatalogService();
            catalogService.Setup(c => c.GetCatalogsList()).Returns(new[] { GetCatalog() });

            var categoryService = GetCategoryService(eventPublisher.Object, cacheManager.Object, catalogService.Object,
                GetValidator(), GetMockedCatalogRepository().Object);

            categoryService.Create(new[] {category});

            AssertValues<CategoryChangingEvent, Category>(eventPublisher, changingEventChangedEntries, EntryState.Added);
            AssertValues<CategoryChangedEvent, Category>(eventPublisher, changedEventChangedEntries, EntryState.Added);

        }

        [Fact]
        public void TestUpdateCategoryEntityEvent()
        {
            var category = GetCategory();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangingEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangedEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var cacheManager = GetMockedCacheManager();

            var catalogService = GetMockedCatalogService();
            catalogService.Setup(c => c.GetCatalogsList()).Returns(new[] { GetCatalog() });

            var catalogRepository = GetMockedCatalogRepository();
            catalogRepository
                .Setup(c => c.GetCategoriesByIds(It.IsAny<string[]>(), It.IsAny<CategoryResponseGroup>()))
                .Returns(new[] {GetCategoryEntity()});

            var categoryService = GetCategoryService(eventPublisher.Object, cacheManager.Object, catalogService.Object,
                GetValidator(), catalogRepository.Object);

            categoryService.Update(new [] {category});

            AssertValues<CategoryChangingEvent, Category>(eventPublisher, changingEventChangedEntries, EntryState.Modified);
            AssertValues<CategoryChangedEvent, Category>(eventPublisher, changedEventChangedEntries, EntryState.Modified);
        }

        [Fact]
        public void TestDeleteCategoryEntityEvent()
        {
            var category = GetCategory();

            var eventPublisher = GetMockedEventPublisher();

            var changingEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangingEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changingEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var changedEventChangedEntries = new List<GenericChangedEntry<Category>>();
            AssignChangedEntriesToLicalVariable<CategoryChangedEvent, Category>(
                eventPublisher,
                (changedEntry, token) => { changedEventChangedEntries = changedEntry.ChangedEntries.ToList(); });

            var cacheManager = GetMockedCacheManager();
            cacheManager
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, Category>
                {
                    {"testCategoryId", category}
                });

            var catalogService = GetMockedCatalogService();

            var catalogRepository = GetMockedCatalogRepository();

            var categoryService = GetCategoryService(eventPublisher.Object, cacheManager.Object, catalogService.Object,
                GetValidator(), catalogRepository.Object);

            categoryService.Delete(new[] {"testCategoryId"});

            AssertValues<CategoryChangingEvent, Category>(eventPublisher, changingEventChangedEntries, EntryState.Deleted);
            AssertValues<CategoryChangedEvent, Category>(eventPublisher, changedEventChangedEntries, EntryState.Deleted);
        }

        private ICategoryService GetCategoryService(IEventPublisher eventPublisher, ICacheManager<object> cacheManager, ICatalogService catalogService, AbstractValidator<IHasProperties> validator, ICatalogRepository catalogRepository)
        {
            return new CategoryServiceImpl(
                () => catalogRepository,
                new Mock<ICommerceService>().Object,
                new Mock<IOutlineService>().Object,
                catalogService,
                cacheManager,
                validator,
                eventPublisher
                );
        }

        private Category GetCategory()
        {
            return new Category
            {
                Id = "testCategoryId",
                CatalogId = "testCatalogId",
                Code = "testCode",
                Name = "testCategoryName",
                Catalog = GetCatalog(),
                Parents = new []
                {
                    new Category()
                }
            };
        }

        private Catalog GetCatalog()
        {
            return new Catalog
            {
                Id = "testCatalogId",
                Languages = new[]
                {
                    new CatalogLanguage
                    {
                        IsDefault = true
                    }
                },
                Properties = new []
                {
                    new Property
                    {
                        Name = "testPropertyName",
                        Type = PropertyType.Category
                    }
                }
            };
        }

        private CategoryEntity GetCategoryEntity()
        {
            return new CategoryEntity
            {
                Id = "testCategoryId",
                CatalogId = "testCatalogId"
            };
        }
    }
}
