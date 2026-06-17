using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport
{
    /// <summary>
    /// Regression coverage for VCST-5278: product export (POST /api/export/data) returned HTTP 500
    /// when a product's image carried an absolute/external <see cref="AssetBase.Url"/> while its
    /// <see cref="AssetBase.RelativeUrl"/> stayed non-absolute. <see cref="AssetBase.HasExternalUrl"/>
    /// is computed from RelativeUrl only, so the "skip external links" guard did NOT skip, and
    /// LoadImages streamed from the FileSystem blob provider using the absolute foreign Url — which
    /// the provider rejects with an invalid local path, throwing and failing the whole export.
    /// </summary>
    public class ProductExportPagedDataSourceLoadImagesTests
    {
        private const string AbsoluteForeignUrl =
            "https://qademovc3.blob.core.windows.net/catalog/x.png";

        // Real production URL class that defeated the original Uri.IsWellFormedUriString guard:
        // an absolute https URL whose path contains unescaped characters (space, em-dash, Cyrillic).
        // IsWellFormedUriString(..., Absolute) returns FALSE for it, so the first fix did NOT skip it
        // and still 500'd; Uri.TryCreate(..., Absolute) returns TRUE, so the corrected guard skips it.
        private const string AbsoluteForeignNonAsciiUrl =
            "https://qademovc3.blob.core.windows.net/catalog/x/p-belii-fon-2 — копия.png";

        private static MethodInfo LoadImagesMethod =>
            typeof(ProductExportPagedDataSource).GetMethod(
                "LoadImages",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private static ProductExportPagedDataSource CreateDataSource(IBlobStorageProvider blobStorageProvider)
        {
            return new ProductExportPagedDataSource(
                blobStorageProvider,
                new Mock<IItemService>().Object,
                new Mock<IProductSearchService>().Object,
                new ProductExportDataQuery());
        }

        /// <summary>
        /// Mirrors the real FileSystem assets provider: OpenRead on an absolute foreign URL is
        /// turned into an invalid local path and throws (the source of the production 500).
        /// </summary>
        private static Mock<IBlobStorageProvider> CreateThrowingProvider()
        {
            var provider = new Mock<IBlobStorageProvider>();
            provider
                .Setup(x => x.OpenRead(It.Is<string>(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))))
                .Throws(() => new DirectoryNotFoundException(
                    "Could not find a part of the path '/opt/virtocommerce/platform/wwwroot/cms-content/assets/https:/qademovc3.blob.core.windows.net/catalog/x.png'."));
            provider
                .Setup(x => x.OpenRead(It.Is<string>(url => !Uri.IsWellFormedUriString(url, UriKind.Absolute))))
                .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));
            return provider;
        }

        /// <summary>
        /// Mirrors the FileSystem assets provider more faithfully for the non-ASCII URL class: it throws
        /// for ANY absolute URL (detected via <see cref="Uri.TryCreate(string, UriKind, out Uri)"/>, which —
        /// unlike <see cref="Uri.IsWellFormedUriString"/> — recognises absolute URLs with unescaped
        /// characters), and returns a real stream for relative local paths.
        /// </summary>
        private static bool IsAbsoluteHttpUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var u)
            && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);

        private static Mock<IBlobStorageProvider> CreateProviderThrowingOnAnyAbsoluteUrl()
        {
            var provider = new Mock<IBlobStorageProvider>();
            provider
                .Setup(x => x.OpenRead(It.Is<string>(url => IsAbsoluteHttpUrl(url))))
                .Throws(() => new DirectoryNotFoundException(
                    "Could not find a part of the path '/opt/virtocommerce/platform/wwwroot/cms-content/assets/https:/qademovc3.blob.core.windows.net/...'."));
            provider
                .Setup(x => x.OpenRead(It.Is<string>(url => !IsAbsoluteHttpUrl(url))))
                .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));
            return provider;
        }

        private static CatalogProduct ProductWithDriftedImage()
        {
            // Legacy/drifted asset: absolute foreign Url, but a non-empty, non-absolute RelativeUrl
            // => HasExternalUrl == false (computed from RelativeUrl only).
            var image = new Image
            {
                Url = AbsoluteForeignUrl,
                RelativeUrl = "catalog/x.png",
            };
            image.HasExternalUrl.Should().BeFalse(
                "the precondition for VCST-5278 is HasExternalUrl == false while Url is absolute");

            return new CatalogProduct { Images = new[] { image } };
        }

        private void InvokeLoadImages(ProductExportPagedDataSource dataSource, CatalogProduct product)
        {
            try
            {
                LoadImagesMethod.Invoke(dataSource, new object[] { new IHasImages[] { product } });
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                throw tie.InnerException;
            }
        }

        [Fact]
        public void LoadImages_AbsoluteForeignUrl_WithRelativeUrl_DoesNotStreamFromLocalStorage()
        {
            // arrange
            var provider = CreateThrowingProvider();
            var dataSource = CreateDataSource(provider.Object);
            var product = ProductWithDriftedImage();

            // act
            Action act = () => InvokeLoadImages(dataSource, product);

            // assert — must NOT throw (production symptom was HTTP 500), and the absolute/external
            // asset is skipped, leaving BinaryData null (the intended external-link behavior).
            act.Should().NotThrow();
            product.Images[0].BinaryData.Should().BeNull();
            provider.Verify(x => x.OpenRead(AbsoluteForeignUrl), Times.Never);
        }

        [Fact]
        public void LoadImages_AbsoluteForeignUrl_WithUnescapedNonAsciiChars_DoesNotStreamFromLocalStorage()
        {
            // Documents the regression class the first fix missed: Uri.IsWellFormedUriString returns
            // FALSE for an absolute URL with unescaped space/em-dash/Cyrillic, so the old guard let it
            // through and the FileSystem provider 500'd. The corrected Uri.TryCreate guard must skip it.
            Uri.IsWellFormedUriString(AbsoluteForeignNonAsciiUrl, UriKind.Absolute).Should().BeFalse(
                "this URL is the regression class that defeated the original IsWellFormedUriString guard");

            // arrange — provider throws on ANY absolute http/https URL, like the real FileSystem provider.
            var provider = CreateProviderThrowingOnAnyAbsoluteUrl();
            var dataSource = CreateDataSource(provider.Object);
            var image = new Image
            {
                Url = AbsoluteForeignNonAsciiUrl,
                RelativeUrl = "catalog/x/p-belii-fon-2 — копия.png",
            };
            image.HasExternalUrl.Should().BeFalse(
                "the precondition for VCST-5278 is HasExternalUrl == false while Url is absolute");
            var product = new CatalogProduct { Images = new[] { image } };

            // act
            Action act = () => InvokeLoadImages(dataSource, product);

            // assert — must NOT throw, asset skipped, BinaryData stays null, never streamed from storage.
            act.Should().NotThrow();
            product.Images[0].BinaryData.Should().BeNull();
            provider.Verify(x => x.OpenRead(AbsoluteForeignNonAsciiUrl), Times.Never);
        }

        [Fact]
        public void LoadImages_LocalRelativeUrl_StillStreamsBinaryData()
        {
            // arrange — a genuine local asset (relative Url) must still be read into BinaryData.
            var provider = CreateThrowingProvider();
            var dataSource = CreateDataSource(provider.Object);
            var image = new Image { Url = "catalog/local.png", RelativeUrl = "catalog/local.png" };
            var product = new CatalogProduct { Images = new[] { image } };

            // act
            InvokeLoadImages(dataSource, product);

            // assert
            image.BinaryData.Should().NotBeNull();
            image.BinaryData.Should().Equal(new byte[] { 1, 2, 3 });
        }
    }
}
