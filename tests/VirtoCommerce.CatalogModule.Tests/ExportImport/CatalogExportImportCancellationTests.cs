using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport
{
    public class CatalogExportImportCancellationTests
    {
        private readonly CatalogExportImport _exportImport;

        public CatalogExportImportCancellationTests()
        {
            var catalogSearchService = new Mock<ICatalogSearchService>();
            catalogSearchService
                .Setup(s => s.SearchAsync(It.IsAny<CatalogSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new CatalogSearchResult());

            var productSearchService = new Mock<IProductSearchService>();
            productSearchService
                .Setup(s => s.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProductSearchResult());

            var categorySearchService = new Mock<ICategorySearchService>();
            categorySearchService
                .Setup(s => s.SearchAsync(It.IsAny<CategorySearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new CategorySearchResult());

            var propertySearchService = new Mock<IPropertySearchService>();
            propertySearchService
                .Setup(s => s.SearchPropertiesAsync(It.IsAny<PropertySearchCriteria>()))
                .ReturnsAsync(new PropertySearchResult());

            var propertyDictionarySearchService = new Mock<IPropertyDictionaryItemSearchService>();
            propertyDictionarySearchService
                .Setup(s => s.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new PropertyDictionaryItemSearchResult());

            var configurationSearchService = new Mock<IProductConfigurationSearchService>();
            configurationSearchService
                .Setup(s => s.SearchAsync(It.IsAny<ProductConfigurationSearchCriteria>()))
                .ReturnsAsync(new ProductConfigurationSearchResult());

            var measureSearchService = new Mock<IMeasureSearchService>();
            measureSearchService
                .Setup(s => s.SearchAsync(It.IsAny<MeasureSearchCriteria>()))
                .ReturnsAsync(new MeasureSearchResult());

            var propertyGroupSearchService = new Mock<IPropertyGroupSearchService>();
            propertyGroupSearchService
                .Setup(s => s.SearchAsync(It.IsAny<PropertyGroupSearchCriteria>()))
                .ReturnsAsync(new PropertyGroupSearchResult());

            _exportImport = new CatalogExportImport(
                new Mock<ICatalogService>().Object,
                catalogSearchService.Object,
                productSearchService.Object,
                categorySearchService.Object,
                new Mock<ICategoryService>().Object,
                new Mock<IItemService>().Object,
                new Mock<IPropertyService>().Object,
                propertySearchService.Object,
                propertyDictionarySearchService.Object,
                new Mock<IPropertyDictionaryItemService>().Object,
                JsonSerializer.CreateDefault(),
                new Mock<IBlobStorageProvider>().Object,
                new Mock<IAssociationService>().Object,
                new Mock<IProductConfigurationService>().Object,
                configurationSearchService.Object,
                new Mock<IMeasureService>().Object,
                measureSearchService.Object,
                new Mock<IPropertyGroupService>().Object,
                propertyGroupSearchService.Object);
        }

        [Fact]
        public async Task DoExportAsync_PreCancelledToken_ThrowsOperationCanceledException()
        {
            //Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            //Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _exportImport.DoExportAsync(Stream.Null, new ExportImportOptions(), _ => { }, cts.Token));
        }

        [Fact]
        public async Task DoImportAsync_PreCancelledToken_ThrowsOperationCanceledException()
        {
            //Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            //Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _exportImport.DoImportAsync(Stream.Null, new ExportImportOptions(), _ => { }, cts.Token));
        }

        [Fact]
#pragma warning disable VC0014
        public async Task DoExportAsync_LegacyOverload_DropsCancellation()
#pragma warning restore VC0014
        {
            //Arrange — mock token that would throw if consulted
            var mockToken = new Mock<ICancellationToken>();
            mockToken.Setup(t => t.ThrowIfCancellationRequested())
                .Throws<OperationCanceledException>();

            //Act
            using var outStream = new MemoryStream();
#pragma warning disable VC0014
            await _exportImport.DoExportAsync(outStream, new ExportImportOptions(), _ => { }, mockToken.Object);
#pragma warning restore VC0014

            //Assert — shim delegates to CancellationToken.None, mock token never consulted
            mockToken.Verify(t => t.ThrowIfCancellationRequested(), Times.Never);
        }

        [Fact]
#pragma warning disable VC0014
        public async Task DoImportAsync_LegacyOverload_DropsCancellation()
#pragma warning restore VC0014
        {
            //Arrange — mock token that would throw if consulted
            var mockToken = new Mock<ICancellationToken>();
            mockToken.Setup(t => t.ThrowIfCancellationRequested())
                .Throws<OperationCanceledException>();

            //Act
            using var inputStream = new MemoryStream();
#pragma warning disable VC0014
            await _exportImport.DoImportAsync(inputStream, new ExportImportOptions(), _ => { }, mockToken.Object);
#pragma warning restore VC0014

            //Assert — shim delegates to CancellationToken.None, mock token never consulted
            mockToken.Verify(t => t.ThrowIfCancellationRequested(), Times.Never);
        }
    }
}
