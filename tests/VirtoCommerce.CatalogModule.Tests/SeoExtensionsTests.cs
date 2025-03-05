using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class SeoExtensionsTests
    {
        private readonly Store _store = new()
        {
            Id = "Store1",
            DefaultLanguage = "en-US",
        };

        [Fact]
        public void GetBestMatchingSeoInfo_WithNullParameters_ReturnsNull()
        {
            // Arrange
            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product2" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product2" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store: null, language: null, slug: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithValidParameters_ReturnsSeoInfo()
        {
            // Arrange
            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product2" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product2" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(_store, language: "en-US", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Store1", result.StoreId);
            Assert.Equal("en-US", result.LanguageCode);
            Assert.Equal("product1", result.SemanticUrl);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithNonExistLang_ReturnsDefaultStoreLang()
        {
            // Arrange
            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = null, SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "fr-FR", SemanticUrl = "product1" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(_store, language: "de-DE", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.LanguageCode, _store.DefaultLanguage);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithNonExistLangAndStoreLang_ReturnsEmptyLang()
        {
            // Arrange
            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "fr-FR", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = null, SemanticUrl = "product1" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(_store, language: "de-DE", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.LanguageCode);
        }
    }
}
