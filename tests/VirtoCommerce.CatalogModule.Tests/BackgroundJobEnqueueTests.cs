using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Handlers;
using VirtoCommerce.CatalogModule.Data.Jobs;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(BackgroundJobEnqueueTests))]
    public class BackgroundJobEnqueueTests
    {
        [Fact]
        public async Task LogChanges_ProductChanged_EnqueuesOneJobWithOneLogPerChangedEntry()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), () => Mock.Of<ICatalogRepository>());

            var message = new ProductChangedEvent(
            [
                new GenericChangedEntry<CatalogProduct>(new CatalogProduct { Id = "p1" }, new CatalogProduct { Id = "p1" }, EntryState.Modified),
                new GenericChangedEntry<CatalogProduct>(new CatalogProduct { Id = "p2" }, new CatalogProduct { Id = "p2" }, EntryState.Added),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            var payload = Assert.IsType<LogEntityChangesJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(["p1", "p2"], payload.OperationLogs.Select(x => x.ObjectId));
        }

        [Fact]
        public async Task TrackSpecialChanges_ParentChanged_EnqueuesTheAffectedCategoryIds()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new TrackSpecialChangesEventHandler(() => Mock.Of<ICatalogRepository>(), Mock.Of<Core.Services.IItemService>());

            var message = new CategoryChangedEvent(
            [
                new GenericChangedEntry<Category>(
                    new Category { Id = "c1", CatalogId = "cat", ParentId = "new-parent" },
                    new Category { Id = "c1", CatalogId = "cat", ParentId = "old-parent" },
                    EntryState.Modified),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            var payload = Assert.IsType<UpdateProductsJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(["c1"], payload.CategoryIds);
        }

        [Fact]
        public async Task TrackSpecialChanges_NothingSpecialChanged_EnqueuesNothing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new TrackSpecialChangesEventHandler(() => Mock.Of<ICatalogRepository>(), Mock.Of<Core.Services.IItemService>());

            var category = new Category { Id = "c1", CatalogId = "cat", ParentId = "parent", IsActive = true };

            //Act
            await handler.Handle(new CategoryChangedEvent([new GenericChangedEntry<Category>(category, category, EntryState.Modified)]));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
        }

        // Captures what a handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
        // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
        // resolves it - the handlers themselves are root-resolved and must never hold it.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                Setup<LogEntityChangesJobHandler>();
                Setup<UpdateProductsJobHandler>();

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public object Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }

            private void Setup<THandler>()
                where THandler : class
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<THandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) =>
                    {
                        Payload = payload;
                        EnqueueCount++;
                    })
                    .ReturnsAsync("job-id");
            }
        }
    }
}
