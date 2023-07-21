using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogDocumentBuilderTests
    {
        [Theory, MemberData(nameof(OutlineData))]
        public void GetOutlineStrings_GetPath(Outline[] outlines, bool getNameLatestItem, string[] expectedResult)
        {
            //Arrange
            var builder = GetFakeCatalogDocumentBuilder();

            //Act
            var result = builder.GetOutlineStringsPublic(outlines, getNameLatestItem);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetOutlineStrings_SomeIdsContainUppercasedChars_OutlinesMustBeInLowercase()
        {
            //Arrange
            var builder = GetFakeCatalogDocumentBuilder();
            var outlines = new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "C", Name = "catalog" },
                                new OutlineItem { Id = "C1", Name = "category1" },
                                new OutlineItem { Id = "S1", Name = "subcategory1" },
                                new OutlineItem { Id = "P1", Name = "product1" },
                            } } };

            //Act
            var result = builder.GetOutlineStringsPublic(outlines, true);

            //Assert
            Assert.DoesNotContain(result, outline => outline.Any(c => char.IsUpper(c)));
        }

        private FakeCatalogDocumentBuilder GetFakeCatalogDocumentBuilder()
        {
            return new FakeCatalogDocumentBuilder(Mock.Of<ISettingsManager>(), Mock.Of<IPropertySearchService>());
        }

        public static IEnumerable<object[]> OutlineData
        {
            get
            {
                return new[]
                {
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "catalog" },
                                new OutlineItem { Id = "11", Name = "category1" },
                                new OutlineItem { Id = "111", Name = "product1" },
                            } } }, false, new [] { "1/11", "1" } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "catalog" },
                                new OutlineItem { Id = "11", Name = "category1" },
                                new OutlineItem { Id = "12", Name = "category2" },
                                new OutlineItem { Id = "111", Name = "product1" },
                            } } }, false, new [] { "1/11/12", "1/11", "1", "1/12" } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "catalog" },
                                new OutlineItem { Id = "11", Name = "category1" },
                                new OutlineItem { Id = "12", Name = "category2" },
                                new OutlineItem { Id = "13", Name = "category3" },
                                new OutlineItem { Id = "111", Name = "product1" },
                            } } }, false, new [] { "1/11/12/13", "1/11/12", "1/11", "1", "1/12", "1/13" } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "Catalog" },
                                new OutlineItem { Id = "11", Name = "Category1" },
                                new OutlineItem { Id = "111", Name = "Product1" },
                            } } }, true,
                        new []
                        {
                            $"1/11{ModuleConstants.OutlineDelimiter}Category1",
                            $"1{ModuleConstants.OutlineDelimiter}Catalog"
                        } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "Catalog" },
                                new OutlineItem { Id = "11", Name = "Category1" },
                                new OutlineItem { Id = "12", Name = "Category2" },
                                new OutlineItem { Id = "111", Name = "Product1" },
                            } } }, true,
                        new []
                        {
                            $"1/11/12{ModuleConstants.OutlineDelimiter}Category2",
                            $"1/11{ModuleConstants.OutlineDelimiter}Category1",
                            $"1{ModuleConstants.OutlineDelimiter}Catalog",
                            $"1/12{ModuleConstants.OutlineDelimiter}Category2"
                        } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "Catalog" },
                                new OutlineItem { Id = "11", Name = "Category1" },
                                new OutlineItem { Id = "12", Name = "Category2" },
                                new OutlineItem { Id = "13", Name = "Category3" },
                                new OutlineItem { Id = "111", Name = "Product1" },
                            } } }, true,
                        new []
                        {
                            $"1/11/12/13{ModuleConstants.OutlineDelimiter}Category3",
                            $"1/11/12{ModuleConstants.OutlineDelimiter}Category2",
                            $"1/11{ModuleConstants.OutlineDelimiter}Category1",
                            $"1{ModuleConstants.OutlineDelimiter}Catalog",
                            $"1/12{ModuleConstants.OutlineDelimiter}Category2",
                            $"1/13{ModuleConstants.OutlineDelimiter}Category3"
                        } },
                };
            }
        }
    }

    class FakeCatalogDocumentBuilder : CatalogDocumentBuilder
    {
        public FakeCatalogDocumentBuilder(ISettingsManager settingsManager, IPropertySearchService propertySearchService)
            : base(settingsManager, propertySearchService)
        {
        }

        public string[] GetOutlineStringsPublic(IEnumerable<Outline> outlines, bool getNameLatestItem = false)
        {
            return GetOutlineStrings(outlines, getNameLatestItem);
        }

        protected override string[] GetOutlineStrings(IEnumerable<Outline> outlines, bool getNameLatestItem = false)
        {
            return base.GetOutlineStrings(outlines, getNameLatestItem);
        }
    }
}
