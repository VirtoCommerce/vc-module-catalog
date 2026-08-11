using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Seo.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport;

public class CatalogExportImportBinaryPackageTests
{
    static CatalogExportImportBinaryPackageTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public async Task DoExportAsync_WithBinaryData_WritesReferencesAndExactSideCarBytes()
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture();
        var graph = CreateCatalogGraph();
        RegisterBlobs(fixture, graph.Blobs);
        fixture.SetCategoryExportResults(graph.Category);
        fixture.SetProductExportResults(graph.Product);

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        package.Manifest.Value<int>("formatVersion").Should().Be(1);
        package.Manifest.Value<string>("catalogEntry").Should().Be("catalog.json");
        package.Manifest.Value<string>("binaryDataDirectory").Should().Be("assets/");

        var binaryObjects = GetBinaryObjects(package.Catalog).ToArray();
        binaryObjects.Should().HaveCount(6);

        foreach (var (url, bytes) in graph.Blobs)
        {
            var jsonAsset = binaryObjects.Single(x => x.Value<string>("RelativeUrl") == url);
            var reference = jsonAsset.Value<string>("BinaryDataReference");

            reference.Should().Be($"assets/{url}", "the source URL hierarchy should be visible in the package");
            jsonAsset["BinaryData"].Should().BeNull("new packages must not contain inline base64 data");
            package.Entries[reference].Should().Equal(bytes);
        }

        package.Entries.Keys.Count(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().Be(6);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoExportAsync_WithBinaryDataWriter_WritesReadableJsonAndExternalSidecars()
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture();
        var graph = CreateCatalogGraph();
        RegisterBlobs(fixture, graph.Blobs);
        fixture.SetCategoryExportResults(graph.Category);
        fixture.SetProductExportResults(graph.Product);
        var binaryDataStore = new TestBinaryDataStore();
        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            binaryDataStore,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var exportedBytes = output.ToArray();
        exportedBytes.Should().StartWith((byte)'{');

        var catalog = JObject.Parse(Encoding.UTF8.GetString(exportedBytes));
        var binaryObjects = GetBinaryObjects(catalog).ToArray();
        binaryObjects.Should().HaveCount(6);

        foreach (var (url, bytes) in graph.Blobs)
        {
            var reference = CatalogPackageTestHelper.CreateReference(url);
            binaryObjects.Single(x => x.Value<string>("RelativeUrl") == url)
                .Value<string>("BinaryDataReference").Should().Be(reference);
            binaryDataStore.Entries[reference].Should().Equal(bytes);
        }

        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_WithBinaryDataReader_RestoresExternalSidecars()
    {
        // Arrange
        const string url = "catalog/external-sidecar.jpg";
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var bytes = "GHIJ"u8.ToArray();
        var product = CreateProduct(
            "product",
            [CreateImage("image", url, binaryReference: reference)],
            []);
        var catalog = JObject.FromObject(new { Products = new[] { product } });
        var binaryDataStore = new TestBinaryDataStore();
        binaryDataStore.Entries.Add(reference, bytes);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(catalog.ToString(Formatting.None)), writable: false),
            binaryDataStore,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[url].Should().Equal(bytes);
        binaryDataStore.ReadCounts[reference].Should().Be(1);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_LegacyHashedBinaryReference_RestoresNestedSidecar()
    {
        // Arrange
        const string url = "catalog/v-accessories/1_5b4cfc96-3fe2-4ee2-a554-a57febd1c666_large.jpeg";
        const string reference = "assets/e2884a07631f8b75262e8f64459a4dd238d12dffcb5574f240374f3619af275e.bin";
        var bytes = "QRST"u8.ToArray();
        var product = CreateProduct(
            "product",
            [CreateImage("image", url, binaryReference: reference)],
            []);
        var package = CatalogPackageTestHelper.Build(
            JObject.FromObject(new { Products = new[] { product } }),
            (reference, bytes));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[url].Should().Equal(bytes);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_ExternalLegacyHashedBinaryReference_ReportsErrorWithoutReadingSidecar()
    {
        // Arrange
        const string url = "catalog/v-accessories/1_5b4cfc96-3fe2-4ee2-a554-a57febd1c666_large.jpeg";
        const string reference = "assets/e2884a07631f8b75262e8f64459a4dd238d12dffcb5574f240374f3619af275e.bin";
        var product = CreateProduct(
            "product",
            [CreateImage("image", url, binaryReference: reference)],
            []);
        var catalog = JObject.FromObject(new { Products = new[] { product } });
        var binaryDataStore = new TestBinaryDataStore();
        binaryDataStore.Entries.Add(reference, "[\\]^"u8.ToArray());
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(catalog.ToString(Formatting.None)), writable: false),
            binaryDataStore,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
        binaryDataStore.ReadCounts.Should().NotContainKey(reference);
        fixture.Progress.SelectMany(x => x.Errors)
            .Should().Contain(x => x.Contains("does not match destination URL", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DoExportAsync_DuplicateUrlAndExternalUrl_DeduplicatesAndSkipsExternalBinary()
    {
        // Arrange
        const string sharedUrl = "catalog/shared.bin";
        const string externalUrl = "https://cdn.example.test/image.jpg";
        var sharedBytes = new byte[] { 11, 12, 13, 14 };
        var fixture = new CatalogExportImportTestFixture();
        fixture.AddBlob(sharedUrl, sharedBytes);

        var product = CreateProduct("product", new List<Image>
        {
            CreateImage("image-1", sharedUrl),
            CreateImage("image-2", sharedUrl),
            CreateImage("image-external", externalUrl),
        }, new List<Asset>
        {
            CreateAsset("asset-shared", sharedUrl),
        });
        fixture.SetProductExportResults(product);

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var binaryObjects = GetBinaryObjects(package.Catalog).ToArray();
        var sharedObjects = binaryObjects.Where(x => x.Value<string>("RelativeUrl") == sharedUrl).ToArray();
        var externalObject = binaryObjects.Single(x => x.Value<string>("RelativeUrl") == externalUrl);
        var expectedReference = CatalogPackageTestHelper.CreateReference(sharedUrl);

        sharedObjects.Should().HaveCount(3);
        sharedObjects.Select(x => x.Value<string>("BinaryDataReference")).Should().OnlyContain(x => x == expectedReference);
        externalObject["BinaryDataReference"].Should().BeNull();
        externalObject["BinaryData"].Should().BeNull();
        package.Entries.Keys.Count(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().Be(1);
        package.Entries[expectedReference].Should().Equal(sharedBytes);
        fixture.BlobReadOpenCounts[sharedUrl].Should().Be(1);
        fixture.BlobReadOpenCounts.Should().NotContainKey(externalUrl);
    }

    [Fact]
    public async Task DoExportAsync_BlobReadFailure_ReportsErrorAndContinuesWithRemainingFiles()
    {
        // Arrange
        const string missingUrl = "catalog/missing.jpg";
        const string availableUrl = "catalog/available.jpg";
        var availableBytes = new byte[] { 21, 22, 23 };
        var fixture = new CatalogExportImportTestFixture();
        fixture.AddBlob(availableUrl, availableBytes);
        fixture.SetProductExportResults(CreateProduct("product", new List<Image>
        {
            CreateImage("missing-image", missingUrl),
            CreateImage("available-image", availableUrl),
        }, new List<Asset>()));

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var binaryObjects = GetBinaryObjects(package.Catalog).ToDictionary(x => x.Value<string>("RelativeUrl"));
        var availableReference = CatalogPackageTestHelper.CreateReference(availableUrl);

        binaryObjects[missingUrl]["BinaryDataReference"].Should().BeNull();
        binaryObjects[availableUrl].Value<string>("BinaryDataReference").Should().Be(availableReference);
        package.Entries.Keys.Where(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().ContainSingle().Which.Should().Be(availableReference);
        package.Entries[availableReference].Should().Equal(availableBytes);
        fixture.BlobReadOpenCounts.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            [missingUrl] = 1,
            [availableUrl] = 1,
        });
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
        fixture.Logger.Invocations
            .Where(x => x.Arguments[0] is LogLevel.Warning)
            .Select(x => x.Arguments[2].ToString())
            .Should().ContainSingle(x => x.Contains(missingUrl, StringComparison.Ordinal));
    }

    [Fact]
    public async Task DoExportAsync_WithoutImagesOrAssets_ExportsWithoutErrorsOrWarnings()
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture();
        fixture.SetProductExportResults(CreateProduct("product", null, null));
        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        GetBinaryObjects(package.Catalog).Should().BeEmpty();
        package.Entries.Keys.Should().NotContain(x => x.StartsWith("assets/", StringComparison.Ordinal));
        fixture.BlobReadOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
        fixture.Logger.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task DoExportAsync_WithBinaryDataWriterAndWithoutImagesOrAssets_WritesReadableJsonOnly()
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture();
        fixture.SetProductExportResults(CreateProduct("product", null, null));
        var binaryDataStore = new TestBinaryDataStore();
        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            binaryDataStore,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var catalog = JObject.Parse(Encoding.UTF8.GetString(output.ToArray()));
        GetBinaryObjects(catalog).Should().BeEmpty();
        binaryDataStore.Entries.Should().BeEmpty();
        fixture.BlobReadOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
        fixture.Logger.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task DoExportAsync_CountProbeDoesNotReadBlobOrPoisonPageExport()
    {
        // Arrange
        const string url = "catalog/count-probe.jpg";
        var bytes = new byte[] { 31, 32, 33 };
        var fixture = new CatalogExportImportTestFixture();
        var countProduct = CreateProduct("count-product",
            new List<Image> { CreateImage("count-image", url) },
            new List<Asset>());
        var pageProduct = CreateProduct("page-product",
            new List<Image> { CreateImage("page-image", url) },
            new List<Asset>());
        var searchCall = 0;
        var readsBeforePage = -1;

        fixture.ProductSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .Returns((ProductSearchCriteria criteria, bool _) =>
            {
                searchCall++;
                if (searchCall == 1)
                {
                    // A count implementation is allowed to return entities. They must not be traversed.
                    return Task.FromResult(new ProductSearchResult
                    {
                        TotalCount = 1,
                        Results = new List<CatalogProduct> { countProduct },
                    });
                }

                if (searchCall == 2)
                {
                    readsBeforePage = fixture.BlobReadOpenCounts.TryGetValue(url, out var count) ? count : 0;
                    fixture.AddBlob(url, bytes);
                    return Task.FromResult(new ProductSearchResult
                    {
                        TotalCount = 1,
                        Results = new List<CatalogProduct> { pageProduct },
                    });
                }

                return Task.FromResult(new ProductSearchResult { TotalCount = 1 });
            });

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var reference = CatalogPackageTestHelper.CreateReference(url);

        readsBeforePage.Should().Be(0, "the count probe must not open blobs");
        fixture.BlobReadOpenCounts[url].Should().Be(1);
        package.Entries[reference].Should().Equal(bytes);
        GetBinaryObjects(package.Catalog).Single().Value<string>("BinaryDataReference").Should().Be(reference);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoExportAsync_MultiplePages_ExportsEachCategoryAndProductExactlyOnce()
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture { BatchSize = 2 };
        var categories = Enumerable.Range(1, 5)
            .Select(index => new Category
            {
                Id = $"category-{index}",
                CatalogId = "catalog",
                Code = $"category-{index}",
                Name = $"Category {index}",
                Images = new List<Image>
                {
                    CreateImage($"category-image-{index}", $"catalog/category-{index}.jpg"),
                },
            })
            .ToArray();
        var products = Enumerable.Range(1, 5)
            .Select(index => CreateProduct(
                $"product-{index}",
                new List<Image>
                {
                    CreateImage($"product-image-{index}", $"catalog/product-{index}.jpg"),
                },
                new List<Asset>()))
            .ToArray();

        foreach (var index in Enumerable.Range(1, 5))
        {
            fixture.AddBlob($"catalog/category-{index}.jpg", [(byte)index]);
            fixture.AddBlob($"catalog/product-{index}.jpg", [(byte)(index + 5)]);
        }

        fixture.SetCategoryExportResults(categories);
        fixture.SetProductExportResults(products);

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var exportedCategoryIds = package.Catalog["Categories"].Values<JObject>().Select(x => x.Value<string>("Id"));
        var exportedProductIds = package.Catalog["Products"].Values<JObject>().Select(x => x.Value<string>("Id"));

        exportedCategoryIds.Should().Equal(categories.Select(x => x.Id));
        exportedProductIds.Should().Equal(products.Select(x => x.Id));
        package.Entries.Keys.Count(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().Be(10);
        fixture.BlobReadOpenCounts.Should().HaveCount(10).And.OnlyContain(x => x.Value == 1);
        fixture.CategorySearchService.Verify(
            x => x.SearchAsync(It.IsAny<CategorySearchCriteria>(), It.IsAny<bool>()),
            Times.Exactly(4));
        fixture.ProductSearchService.Verify(
            x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()),
            Times.Exactly(4));
    }

    [Fact]
    public async Task DoExportAsync_AssociationObjectImage_DoesNotCreateOrphanSideCar()
    {
        // Arrange
        const string productUrl = "catalog/product-image.jpg";
        const string associatedObjectUrl = "catalog/associated-object-image.jpg";
        var fixture = new CatalogExportImportTestFixture();
        fixture.AddBlob(productUrl, new byte[] { 1, 2, 3 });
        fixture.AddBlob(associatedObjectUrl, new byte[] { 4, 5, 6 });

        var associatedProduct = CreateProduct("associated",
            new List<Image> { CreateImage("associated-image", associatedObjectUrl) },
            new List<Asset>());
        var product = CreateProduct("product",
            new List<Image> { CreateImage("product-image", productUrl) },
            new List<Asset>());
        product.Associations = new List<ProductAssociation>
        {
            new()
            {
                Id = "association",
                ItemId = product.Id,
                AssociatedObjectId = associatedProduct.Id,
                AssociatedObjectType = "product",
                AssociatedObject = associatedProduct,
            },
        };
        fixture.SetProductExportResults(product);

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var productReference = CatalogPackageTestHelper.CreateReference(productUrl);

        package.Entries.Keys.Where(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().ContainSingle().Which.Should().Be(productReference);
        package.Entries[productReference].Should().Equal(1, 2, 3);
        fixture.BlobReadOpenCounts.Keys.Should().BeEquivalentTo(new[] { productUrl });
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoExportAsync_RelativeUrlWinsAndAbsoluteSchemesAreSkipped()
    {
        // Arrange
        const string imageRelativeUrl = "catalog/preferred-image.jpg";
        const string imageDifferentUrl = "catalog/different-image.jpg";
        const string assetRelativeUrl = "catalog/preferred-file.pdf";
        const string assetDifferentUrl = "catalog/different-file.pdf";
        const string customSchemeUrl = "s3://catalog-bucket/external-image.jpg";
        const string customSchemeFallbackUrl = "catalog/must-not-fallback-s3.jpg";
        const string httpsUrl = "https://cdn.example.test/external-image.jpg";
        const string httpsFallbackUrl = "catalog/must-not-fallback-https.jpg";
        const string schemeRelativeUrl = "//cdn.example.test/external-file.pdf";
        const string schemeRelativeFallbackUrl = "catalog/must-not-fallback-cdn.pdf";
        var fixture = new CatalogExportImportTestFixture();

        var preferredImage = CreateImage("preferred-image", imageDifferentUrl);
        preferredImage.RelativeUrl = imageRelativeUrl;
        var preferredAsset = CreateAsset("preferred-asset", assetDifferentUrl);
        preferredAsset.RelativeUrl = assetRelativeUrl;
        var externalImage = CreateImage("s3-image", customSchemeFallbackUrl);
        externalImage.RelativeUrl = customSchemeUrl;
        var httpsImage = CreateImage("https-image", httpsFallbackUrl);
        httpsImage.RelativeUrl = httpsUrl;
        var externalAsset = CreateAsset("cdn-asset", schemeRelativeFallbackUrl);
        externalAsset.RelativeUrl = schemeRelativeUrl;

        fixture.SetProductExportResults(CreateProduct("product",
            new List<Image> { preferredImage, externalImage, httpsImage },
            new List<Asset> { preferredAsset, externalAsset }));
        fixture.AddBlob(imageRelativeUrl, new byte[] { 1 });
        fixture.AddBlob(imageDifferentUrl, new byte[] { 2 });
        fixture.AddBlob(assetRelativeUrl, new byte[] { 3 });
        fixture.AddBlob(assetDifferentUrl, new byte[] { 4 });
        fixture.AddBlob(customSchemeUrl, new byte[] { 5 });
        fixture.AddBlob(customSchemeFallbackUrl, new byte[] { 6 });
        fixture.AddBlob(httpsUrl, new byte[] { 7 });
        fixture.AddBlob(httpsFallbackUrl, new byte[] { 8 });
        fixture.AddBlob(schemeRelativeUrl, new byte[] { 9 });
        fixture.AddBlob(schemeRelativeFallbackUrl, new byte[] { 10 });

        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var objects = GetBinaryObjects(package.Catalog).ToDictionary(x => x.Value<string>("Id"), StringComparer.Ordinal);

        objects[preferredImage.Id].Value<string>("Url").Should().Be(imageRelativeUrl);
        objects[preferredImage.Id].Value<string>("BinaryDataReference").Should().Be(CatalogPackageTestHelper.CreateReference(imageRelativeUrl));
        objects[preferredAsset.Id].Value<string>("Url").Should().Be(assetRelativeUrl);
        objects[preferredAsset.Id].Value<string>("BinaryDataReference").Should().Be(CatalogPackageTestHelper.CreateReference(assetRelativeUrl));
        objects[externalImage.Id]["BinaryDataReference"].Should().BeNull();
        objects[httpsImage.Id]["BinaryDataReference"].Should().BeNull();
        objects[externalAsset.Id]["BinaryDataReference"].Should().BeNull();
        fixture.BlobReadOpenCounts.Keys.Should().BeEquivalentTo(new[] { imageRelativeUrl, assetRelativeUrl });
        package.Entries.Keys.Count(x => x.StartsWith("assets/", StringComparison.Ordinal)).Should().Be(2);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task ExportThenImport_WithCategoryProductAndVariation_RestoresEveryImageAndAsset()
    {
        // Arrange
        var exportFixture = new CatalogExportImportTestFixture();
        var graph = CreateCatalogGraph();
        RegisterBlobs(exportFixture, graph.Blobs);
        exportFixture.SetCategoryExportResults(graph.Category);
        exportFixture.SetProductExportResults(graph.Product);

        using var output = new MemoryStream();
        await exportFixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            exportFixture.CaptureProgress,
            CancellationToken.None);

        var importFixture = new CatalogExportImportTestFixture();
        var savedProductIds = new List<string>();
        var savedCategoryIds = new List<string>();
        importFixture.ItemService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()))
            .Callback((IList<CatalogProduct> items) => savedProductIds.AddRange(items.Select(x => x.Id)))
            .Returns(Task.CompletedTask);
        importFixture.CategoryService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Category>>()))
            .Callback((IList<Category> items) => savedCategoryIds.AddRange(items.Select(x => x.Id)))
            .Returns(Task.CompletedTask);

        using var input = new MemoryStream(output.ToArray(), writable: false);

        // Act
        await importFixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            importFixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        savedCategoryIds.Should().ContainSingle().Which.Should().Be(graph.Category.Id);
        savedProductIds.Should().BeEquivalentTo(new[] { graph.Product.Id, graph.Product.Variations.Single().Id });
        importFixture.WrittenBlobs.Keys.Should().BeEquivalentTo(graph.Blobs.Keys);
        foreach (var (url, bytes) in graph.Blobs)
        {
            importFixture.WrittenBlobs[url].Should().Equal(bytes);
        }

        importFixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_LegacyJsonOnNonSeekableStream_RestoresInlineImageAndAssetData()
    {
        // Arrange
        var parentImageBytes = new byte[] { 1, 3, 5, 7 };
        var parentAssetBytes = new byte[] { 2, 4, 6, 8 };
        var variationImageBytes = new byte[] { 9, 10, 11 };
        var variationAssetBytes = new byte[] { 12, 13, 14 };
        var variation = CreateVariation("variation", "legacy/variation-image.jpg", "legacy/variation-file.pdf");
        variation.Images[0].BinaryData = variationImageBytes;
        variation.Assets[0].BinaryData = variationAssetBytes;
        var product = CreateProduct("product",
            new List<Image> { CreateImage("parent-image", "legacy/product-image.jpg", parentImageBytes) },
            new List<Asset> { CreateAsset("parent-asset", "legacy/product-file.pdf", parentAssetBytes) },
            variation);

        var json = JsonConvert.SerializeObject(new { Products = new[] { product } }, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
        var body = Encoding.UTF8.GetBytes("\r\n  " + json);
        var legacyBytes = Encoding.UTF8.GetPreamble().Concat(body).ToArray();
        var input = new NonSeekableReadStream(legacyBytes, maxChunkSize: 7);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs["legacy/product-image.jpg"].Should().Equal(parentImageBytes);
        fixture.WrittenBlobs["legacy/product-file.pdf"].Should().Equal(parentAssetBytes);
        fixture.WrittenBlobs["legacy/variation-image.jpg"].Should().Equal(variationImageBytes);
        fixture.WrittenBlobs["legacy/variation-file.pdf"].Should().Equal(variationAssetBytes);
        input.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DoImportAsync_InlineDataAndReference_PrefersLegacyInlineData()
    {
        // Arrange
        const string url = "catalog/legacy-precedence.jpg";
        var inlineBytes = new byte[] { 21, 22, 23 };
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", url, inlineBytes, "assets/not-a-valid-reference.bin") },
            new List<Asset>());
        var package = CatalogPackageTestHelper.Build(JObject.FromObject(new { Products = new[] { product } }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[url].Should().Equal(inlineBytes);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_RelativeUrlWinsAndAbsoluteSchemesAreSkipped()
    {
        // Arrange
        const string imageRelativeUrl = "catalog/import-preferred-image.jpg";
        const string imageDifferentUrl = "catalog/import-different-image.jpg";
        const string assetRelativeUrl = "catalog/import-preferred-file.pdf";
        const string assetDifferentUrl = "catalog/import-different-file.pdf";
        const string customSchemeUrl = "s3://catalog-bucket/import-external-image.jpg";
        const string customSchemeFallbackUrl = "catalog/import-must-not-fallback-s3.jpg";
        const string httpsUrl = "https://cdn.example.test/import-external-image.jpg";
        const string httpsFallbackUrl = "catalog/import-must-not-fallback-https.jpg";
        const string schemeRelativeUrl = "//cdn.example.test/import-external-file.pdf";
        const string schemeRelativeFallbackUrl = "catalog/import-must-not-fallback-cdn.pdf";

        var imageReference = CatalogPackageTestHelper.CreateReference(imageRelativeUrl);
        var assetReference = CatalogPackageTestHelper.CreateReference(assetRelativeUrl);
        const string customSchemeReference = "assets/external/s3-image.jpg";
        const string httpsReference = "assets/external/https-image.jpg";
        const string schemeRelativeReference = "assets/external/cdn-file.pdf";

        var preferredImage = CreateImage("preferred-image", imageDifferentUrl, binaryReference: imageReference);
        preferredImage.RelativeUrl = imageRelativeUrl;
        var preferredAsset = CreateAsset("preferred-asset", assetDifferentUrl, binaryReference: assetReference);
        preferredAsset.RelativeUrl = assetRelativeUrl;
        var customSchemeImage = CreateImage("s3-image", customSchemeFallbackUrl, binaryReference: customSchemeReference);
        customSchemeImage.RelativeUrl = customSchemeUrl;
        var httpsImage = CreateImage("https-image", httpsFallbackUrl, binaryReference: httpsReference);
        httpsImage.RelativeUrl = httpsUrl;
        var schemeRelativeAsset = CreateAsset("cdn-asset", schemeRelativeFallbackUrl, binaryReference: schemeRelativeReference);
        schemeRelativeAsset.RelativeUrl = schemeRelativeUrl;

        var product = CreateProduct("product",
            new List<Image> { preferredImage, customSchemeImage, httpsImage },
            new List<Asset> { preferredAsset, schemeRelativeAsset });
        var package = CatalogPackageTestHelper.Build(
            JObject.FromObject(new { Products = new[] { product } }),
            (imageReference, new byte[] { 1, 2 }),
            (assetReference, new byte[] { 3, 4 }),
            (customSchemeReference, new byte[] { 5 }),
            (httpsReference, new byte[] { 6 }),
            (schemeRelativeReference, new byte[] { 7 }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[imageRelativeUrl].Should().Equal(1, 2);
        fixture.WrittenBlobs[assetRelativeUrl].Should().Equal(3, 4);
        fixture.BlobWriteOpenCounts.Keys.Should().BeEquivalentTo(new[] { imageRelativeUrl, assetRelativeUrl });
        fixture.WrittenBlobs.Should().NotContainKey(imageDifferentUrl);
        fixture.WrittenBlobs.Should().NotContainKey(assetDifferentUrl);
        fixture.WrittenBlobs.Should().NotContainKey(customSchemeFallbackUrl);
        fixture.WrittenBlobs.Should().NotContainKey(httpsFallbackUrl);
        fixture.WrittenBlobs.Should().NotContainKey(schemeRelativeFallbackUrl);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task DoImportAsync_SameReferenceAndDestination_WritesBlobOnce()
    {
        // Arrange
        const string url = "catalog/shared-import.bin";
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var bytes = new byte[] { 41, 42, 43, 44 };
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", url, binaryReference: reference) },
            new List<Asset> { CreateAsset("asset", url, binaryReference: reference) });
        var package = CatalogPackageTestHelper.Build(
            JObject.FromObject(new { Products = new[] { product } }),
            (reference, bytes));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts[url].Should().Be(1);
        fixture.WrittenBlobs[url].Should().Equal(bytes);
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();
    }

    [Fact]
    public async Task HandleBinaryDataFalse_UsesRawJsonAndDoesNotReadOrWriteBlobs()
    {
        // Arrange
        const string url = "catalog/not-copied.jpg";
        var exportFixture = new CatalogExportImportTestFixture();
        exportFixture.SetProductExportResults(CreateProduct("product",
            new List<Image> { CreateImage("image", url, new byte[] { 1, 2, 3 }) },
            new List<Asset>()));

        using var output = new MemoryStream();

        // Act
        await exportFixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = false },
            exportFixture.CaptureProgress,
            CancellationToken.None);

        var exportedBytes = output.ToArray();
        var importFixture = new CatalogExportImportTestFixture();
        var input = new NonSeekableReadStream(exportedBytes);
        await importFixture.CreateSut().DoImportAsync(
            input,
            new ExportImportOptions { HandleBinaryData = false },
            importFixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        exportedBytes.Take(2).Should().NotEqual(new byte[] { 0x50, 0x4B });
        var json = JObject.Parse(Encoding.UTF8.GetString(exportedBytes));
        var image = (JObject)json["Products"]![0]!["Images"]![0]!;
        image["BinaryData"].Should().BeNull();
        image["BinaryDataReference"].Should().BeNull();
        exportFixture.BlobReadOpenCounts.Should().BeEmpty();
        importFixture.BlobWriteOpenCounts.Should().BeEmpty();
        importFixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Once);
    }

    [Fact]
    public async Task DoImportAsync_HandleBinaryDataFalse_IgnoresLegacyProductAndCategoryBinaryData()
    {
        // Arrange
        const string categoryImageUrl = "catalog/category.jpg";
        const string categoryAssetUrl = "catalog/category.pdf";
        const string productImageUrl = "catalog/product.jpg";
        const string productAssetUrl = "catalog/product.pdf";
        var category = new Category
        {
            Id = "category",
            CatalogId = "catalog",
            Code = "category",
            Name = "Category",
            Images = new List<Image>
            {
                CreateImage("category-image", categoryImageUrl, new byte[] { 1 }, CatalogPackageTestHelper.CreateReference(categoryImageUrl)),
            },
            Assets = new List<Asset>
            {
                CreateAsset("category-asset", categoryAssetUrl, new byte[] { 2 }, CatalogPackageTestHelper.CreateReference(categoryAssetUrl)),
            },
            Links = new List<CategoryLink>(),
            SeoInfos = new List<SeoInfo>
            {
                new() { SemanticUrl = "category", PageTitle = "Category", LanguageCode = "en-US" },
            },
        };
        var product = CreateProduct("product",
            new List<Image>
            {
                CreateImage("product-image", productImageUrl, new byte[] { 3 }, CatalogPackageTestHelper.CreateReference(productImageUrl)),
            },
            new List<Asset>
            {
                CreateAsset("product-asset", productAssetUrl, new byte[] { 4 }, CatalogPackageTestHelper.CreateReference(productAssetUrl)),
            });
        var json = JsonConvert.SerializeObject(new
        {
            Categories = new[] { category },
            Products = new[] { product },
        }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        var fixture = new CatalogExportImportTestFixture();
        Category savedCategory = null;
        CatalogProduct savedProduct = null;
        fixture.CategoryService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Category>>()))
            .Callback((IList<Category> items) => savedCategory = items.Single())
            .Returns(Task.CompletedTask);
        fixture.ItemService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()))
            .Callback((IList<CatalogProduct> items) => savedProduct = items.Single())
            .Returns(Task.CompletedTask);

        // Act
        await fixture.CreateSut().DoImportAsync(
            new NonSeekableReadStream(Encoding.UTF8.GetBytes(json)),
            new ExportImportOptions { HandleBinaryData = false },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
        fixture.CategoryService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Category>>()), Times.Once);
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Once);
        savedCategory.Should().NotBeNull();
        savedCategory.Images.Should().OnlyContain(x => x.BinaryData == null && x.BinaryDataReference == null);
        savedCategory.Assets.Should().OnlyContain(x => x.BinaryData == null && x.BinaryDataReference == null);
        savedProduct.Should().NotBeNull();
        savedProduct.Images.Should().OnlyContain(x => x.BinaryData == null && x.BinaryDataReference == null);
        savedProduct.Assets.Should().OnlyContain(x => x.BinaryData == null && x.BinaryDataReference == null);
    }

    [Theory]
    [InlineData("../outside.bin")]
    [InlineData("assets/../outside.bin")]
    [InlineData("assets/./inside.bin")]
    [InlineData("assets//inside.bin")]
    [InlineData("assets/")]
    [InlineData("/assets/0000000000000000000000000000000000000000000000000000000000000000.bin")]
    [InlineData("assets\\0000000000000000000000000000000000000000000000000000000000000000.bin")]
    [InlineData("catalog.json")]
    public async Task DoImportAsync_InvalidBinaryReference_ReportsErrorWithoutOpeningBlob(string invalidReference)
    {
        // Arrange
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", "catalog/image.jpg", binaryReference: invalidReference) },
            new List<Asset>());
        var package = CatalogPackageTestHelper.Build(JObject.FromObject(new { Products = new[] { product } }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().Contain(x => x.Contains("invalid", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("../outside.jpg")]
    [InlineData("catalog/../outside.jpg")]
    [InlineData("catalog/./image.jpg")]
    [InlineData("catalog//image.jpg")]
    [InlineData("catalog\\image.jpg")]
    public async Task DoExportAsync_UnsafeSourceUrl_DoesNotCreateSideCar(string sourceUrl)
    {
        // Arrange
        var fixture = new CatalogExportImportTestFixture();
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", sourceUrl) },
            new List<Asset>());
        fixture.SetProductExportResults(product);
        using var output = new MemoryStream();

        // Act
        await fixture.CreateSut().DoExportAsync(
            output,
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        var package = CatalogPackageTestHelper.Read(output.ToArray());
        var image = GetBinaryObjects(package.Catalog).Single();
        image["BinaryDataReference"].Should().BeNull();
        package.Entries.Keys.Should().NotContain(x => x.StartsWith("assets/", StringComparison.Ordinal));
        fixture.BlobReadOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().BeEmpty();

        var warning = fixture.Logger.Invocations.Single(x => x.Arguments[0] is LogLevel.Warning);
        warning.Arguments[2].ToString().Should().Contain(sourceUrl);
        warning.Arguments[3].Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("safe package path");
    }

    [Fact]
    public async Task DoImportAsync_BinaryReferenceForDifferentUrl_ReportsErrorWithoutOpeningBlob()
    {
        // Arrange
        const string destinationUrl = "catalog/destination.jpg";
        var foreignReference = CatalogPackageTestHelper.CreateReference("catalog/different.jpg");
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", destinationUrl, binaryReference: foreignReference) },
            new List<Asset>());
        var package = CatalogPackageTestHelper.Build(
            JObject.FromObject(new { Products = new[] { product } }),
            (foreignReference, new byte[] { 1, 2, 3 }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().Contain(x => x.Contains("does not match destination URL", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("{\"formatVersion\":2,\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\"}")]
    [InlineData("{\"formatVersion\":\"1\",\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\"}")]
    [InlineData("{\"formatVersion\":1,\"catalogEntry\":\"catalog.json\"}")]
    [InlineData("{\"formatVersion\":1,\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\",\"extra\":true}")]
    [InlineData("{\"formatVersion\":1,\"formatVersion\":1,\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\"}")]
    [InlineData("{\"formatVersion\":1,\"catalogEntry\":\"catalog.json\",\"binaryDataDirectory\":\"assets/\"}{}")]
    public async Task DoImportAsync_InvalidManifest_RejectsPackageBeforeSaving(string manifest)
    {
        // Arrange
        var package = CatalogPackageTestHelper.BuildWithManifest(new JObject(), manifest);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>();
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_OversizedManifest_RejectsPackageBeforeSaving()
    {
        // Arrange
        var package = CatalogPackageTestHelper.BuildWithManifest(new JObject(), new string('x', 4097));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*manifest is too large*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_MissingManifest_RejectsPackageBeforeSaving()
    {
        // Arrange
        var package = CatalogPackageTestHelper.BuildEntries(
            ("catalog.json", Encoding.UTF8.GetBytes("{}"), CompressionLevel.NoCompression));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*package.json*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_MissingCatalogEntry_RejectsPackageBeforeSaving()
    {
        // Arrange
        var package = CatalogPackageTestHelper.BuildEntries(
            ("package.json", Encoding.UTF8.GetBytes(CatalogPackageTestHelper.ManifestJson), CompressionLevel.NoCompression));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*catalog.json*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_UnsupportedArchiveEntry_RejectsPackageBeforeSaving()
    {
        // Arrange
        var package = CatalogPackageTestHelper.BuildEntries(
            ("package.json", Encoding.UTF8.GetBytes(CatalogPackageTestHelper.ManifestJson), CompressionLevel.NoCompression),
            ("catalog.json", Encoding.UTF8.GetBytes("{}"), CompressionLevel.NoCompression),
            ("unexpected.txt", new byte[] { 1 }, CompressionLevel.NoCompression));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*unsupported entry*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_CompressedInnerEntry_RejectsPackageBeforeSaving()
    {
        // Arrange
        var catalog = new JObject { ["padding"] = new string('x', 8192) };
        var package = CatalogPackageTestHelper.BuildWithManifest(catalog, CatalogPackageTestHelper.ManifestJson, CompressionLevel.SmallestSize);
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*unsupported compression*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public void AssetBase_NullBinaryDataReference_IsOmittedEvenWhenSerializerIncludesNulls()
    {
        // Arrange
        var image = CreateImage("image", "catalog/image.jpg");

        // Act
        var json = JObject.Parse(JsonConvert.SerializeObject(image, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
        }));

        // Assert
        json.Should().NotContainKey(nameof(AssetBase.BinaryDataReference));
    }

    [Fact]
    public async Task DoImportAsync_MissingBinaryEntry_ReportsErrorAndContinues()
    {
        // Arrange
        const string url = "catalog/missing.jpg";
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", url, binaryReference: reference) },
            new List<Asset>());
        var package = CatalogPackageTestHelper.Build(JObject.FromObject(new { Products = new[] { product } }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.BlobWriteOpenCounts.Should().BeEmpty();
        fixture.Progress.SelectMany(x => x.Errors).Should().Contain(x => x.Contains("does not contain binary data entry", StringComparison.Ordinal));
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Once);
    }

    [Fact]
    public async Task DoImportAsync_DuplicateArchiveEntry_RejectsPackageBeforeSaving()
    {
        // Arrange
        const string url = "catalog/duplicate.jpg";
        var reference = CatalogPackageTestHelper.CreateReference(url);
        var product = CreateProduct("product",
            new List<Image> { CreateImage("image", url, binaryReference: reference) },
            new List<Asset>());
        var catalog = JObject.FromObject(new { Products = new[] { product } });
        var package = CatalogPackageTestHelper.Build(
            catalog,
            (reference, new byte[] { 1 }),
            (reference, new byte[] { 2 }));
        var fixture = new CatalogExportImportTestFixture();

        // Act
        var act = () => fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*duplicate entry*");
        fixture.ItemService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
    }

    [Fact]
    public async Task DoImportAsync_SkipItem_UploadsBinaryDataOnlyForSuccessfullySavedItem()
    {
        // Arrange
        const string goodUrl = "catalog/good.jpg";
        const string badUrl = "catalog/bad.jpg";
        var goodReference = CatalogPackageTestHelper.CreateReference(goodUrl);
        var badReference = CatalogPackageTestHelper.CreateReference(badUrl);
        var good = CreateProduct("good",
            new List<Image> { CreateImage("good-image", goodUrl, binaryReference: goodReference) },
            new List<Asset>());
        var bad = CreateProduct("bad",
            new List<Image> { CreateImage("bad-image", badUrl, binaryReference: badReference) },
            new List<Asset>());
        var package = CatalogPackageTestHelper.Build(
            JObject.FromObject(new { Products = new[] { good, bad } }),
            (goodReference, new byte[] { 1, 2, 3 }),
            (badReference, new byte[] { 4, 5, 6 }));

        var fixture = new CatalogExportImportTestFixture
        {
            BatchSize = 2,
            ErrorPolicy = OnImportError.SkipItem,
        };
        fixture.ItemService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()))
            .Returns((IList<CatalogProduct> items) =>
            {
                if (items.Count > 1 || items[0].Id == "bad")
                {
                    return Task.FromException(new InvalidOperationException("rejected test item"));
                }

                return Task.CompletedTask;
            });

        // Act
        await fixture.CreateSut().DoImportAsync(
            new MemoryStream(package, writable: false),
            new ExportImportOptions { HandleBinaryData = true },
            fixture.CaptureProgress,
            CancellationToken.None);

        // Assert
        fixture.WrittenBlobs[goodUrl].Should().Equal(1, 2, 3);
        fixture.WrittenBlobs.Should().NotContainKey(badUrl);
        fixture.Progress.SelectMany(x => x.Errors).Should().Contain(x => x.Contains("bad", StringComparison.Ordinal));
    }

    private static CatalogGraph CreateCatalogGraph()
    {
        var blobs = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["catalog/category-image.jpg"] = new byte[] { 1, 2, 3 },
            ["catalog/category-file.pdf"] = new byte[] { 4, 5, 6, 7 },
            ["catalog/product-image.jpg"] = new byte[] { 8, 9 },
            ["catalog/product-file.pdf"] = new byte[] { 10, 11, 12 },
            ["catalog/variation-image.jpg"] = new byte[] { 13, 14, 15, 16 },
            ["catalog/variation-file.pdf"] = Array.Empty<byte>(),
        };

        var category = new Category
        {
            Id = "category",
            CatalogId = "catalog",
            Code = "category",
            Name = "Category",
            Level = 0,
            Images = new List<Image> { CreateImage("category-image", "catalog/category-image.jpg") },
            Assets = new List<Asset> { CreateAsset("category-asset", "catalog/category-file.pdf") },
            Links = new List<CategoryLink>(),
            SeoInfos = new List<SeoInfo>
            {
                new() { SemanticUrl = "category", PageTitle = "Category", LanguageCode = "en-US" },
            },
        };

        var variation = CreateVariation("variation", "catalog/variation-image.jpg", "catalog/variation-file.pdf");
        var product = CreateProduct("product",
            new List<Image> { CreateImage("product-image", "catalog/product-image.jpg") },
            new List<Asset> { CreateAsset("product-asset", "catalog/product-file.pdf") },
            variation);

        return new CatalogGraph(category, product, blobs);
    }

    private static CatalogProduct CreateProduct(string id, IList<Image> images, IList<Asset> assets, params Variation[] variations)
    {
        return new CatalogProduct
        {
            Id = id,
            CatalogId = "catalog",
            Code = id,
            Name = id,
            Images = images,
            Assets = assets,
            Variations = variations.Length == 0 ? null : variations.ToList(),
        };
    }

    private static Variation CreateVariation(string id, string imageUrl, string assetUrl)
    {
        return new Variation
        {
            Id = id,
            CatalogId = "catalog",
            Code = id,
            Name = id,
            Images = new List<Image> { CreateImage($"{id}-image", imageUrl) },
            Assets = new List<Asset> { CreateAsset($"{id}-asset", assetUrl) },
        };
    }

    private static Image CreateImage(string id, string url, byte[] binaryData = null, string binaryReference = null)
    {
        return new Image
        {
            Id = id,
            Name = id,
            Url = url,
            RelativeUrl = url,
            BinaryData = binaryData,
            BinaryDataReference = binaryReference,
        };
    }

    private static Asset CreateAsset(string id, string url, byte[] binaryData = null, string binaryReference = null)
    {
        return new Asset
        {
            Id = id,
            Name = id,
            Url = url,
            RelativeUrl = url,
            BinaryData = binaryData,
            BinaryDataReference = binaryReference,
        };
    }

    private static IEnumerable<JObject> GetBinaryObjects(JObject catalog)
    {
        return catalog.SelectTokens("$..Images[*]")
            .Concat(catalog.SelectTokens("$..Assets[*]"))
            .OfType<JObject>();
    }

    private static void RegisterBlobs(CatalogExportImportTestFixture fixture, IReadOnlyDictionary<string, byte[]> blobs)
    {
        foreach (var (url, bytes) in blobs)
        {
            fixture.AddBlob(url, bytes);
        }
    }

    private sealed class TestBinaryDataStore : IExportBinaryDataWriter, IImportBinaryDataReader
    {
        public Dictionary<string, byte[]> Entries { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int> ReadCounts { get; } = new(StringComparer.Ordinal);

        public async Task WriteAsync(string reference, Stream sourceStream, CancellationToken cancellationToken)
        {
            using var output = new MemoryStream();
            await sourceStream.CopyToAsync(output, cancellationToken);
            Entries.Add(reference, output.ToArray());
        }

        public Task<Stream> OpenReadAsync(string reference, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCounts.TryGetValue(reference, out var readCount);
            ReadCounts[reference] = readCount + 1;
            return Task.FromResult<Stream>(Entries.TryGetValue(reference, out var bytes)
                ? new MemoryStream(bytes, writable: false)
                : null);
        }
    }

    private sealed record CatalogGraph(
        Category Category,
        CatalogProduct Product,
        IReadOnlyDictionary<string, byte[]> Blobs);
}
