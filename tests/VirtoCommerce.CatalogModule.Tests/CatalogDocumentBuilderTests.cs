using System.Collections.Generic;
using Moq;
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

        private FakeCatalogDocumentBuilder GetFakeCatalogDocumentBuilder()
        {
            return new FakeCatalogDocumentBuilder(Mock.Of<ISettingsManager>());
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
                            } } }, true, new [] { "1/11_Category1", "1_Catalog" } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "Catalog" },
                                new OutlineItem { Id = "11", Name = "Category1" },
                                new OutlineItem { Id = "12", Name = "Category2" },
                                new OutlineItem { Id = "111", Name = "Product1" },
                            } } }, true, new [] { "1/11/12_Category2", "1/11_Category1", "1_Catalog", "1/12_Category2" } },
                    new object[] {
                        new[] { new Outline
                        {
                            Items = new[] {
                                new OutlineItem { Id = "1", Name = "Catalog" },
                                new OutlineItem { Id = "11", Name = "Category1" },
                                new OutlineItem { Id = "12", Name = "Category2" },
                                new OutlineItem { Id = "13", Name = "Category3" },
                                new OutlineItem { Id = "111", Name = "Product1" },
                            } } }, true, new [] { "1/11/12/13_Category3", "1/11/12_Category2", "1/11_Category1", "1_Catalog", "1/12_Category2", "1/13_Category3" } },
                };
            }
        }
    }

    class FakeCatalogDocumentBuilder : CatalogDocumentBuilder
    {
        public FakeCatalogDocumentBuilder(ISettingsManager settingsManager) : base(settingsManager)
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
