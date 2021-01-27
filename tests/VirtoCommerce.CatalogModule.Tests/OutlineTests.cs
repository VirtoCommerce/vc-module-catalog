using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.OutlinePart;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CoreModule.Core.Outlines;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class OutlineTests
    {
        [Fact]
        public void GetCategories_When_NoParentsAndNoLinks_Expect_SinglePhysicalOutline()
        {
            var service = GetOutlineService();
            var c0 = GetTestData().OfType<Category>().FirstOrDefault(x => x.Id == "c0");
            service.FillOutlinesForObjects(new[] { c0 }, null);


            Assert.NotNull(c0.Outlines);
            Assert.Equal(1, c0.Outlines.Count);

            var outline = c0.Outlines.First();
            Assert.NotNull(outline.Items);
            Assert.Equal(2, outline.Items.Count);

            var outlineString = outline.ToString();
            Assert.Equal("c/c0", outlineString);

            //var allOutlineItems = c0.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetCategories_When_PhysicalCatalog_Expect_SinglePhysicalOutline()
        {
            var bol = false;
            bol |= true;
            var service = GetOutlineService();
            var c3 = GetTestData().OfType<Category>().FirstOrDefault(x => x.Id == "c3");
            service.FillOutlinesForObjects(new[] { c3 }, "c");

            Assert.NotNull(c3.Outlines);
            Assert.Equal(1, c3.Outlines.Count);

            var outline = c3.Outlines.First();
            Assert.NotNull(outline.Items);
            Assert.Equal(4, outline.Items.Count);

            var outlineString = outline.ToString();
            Assert.Equal("c/c1/c2/c3", outlineString);

            //var allOutlineItems = category.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetCategories_When_VirtualCatalog_Expect_MultipleVirtualOutlines()
        {
            var service = GetOutlineService();
            var c3 = GetTestData().OfType<Category>().FirstOrDefault(x => x.Id == "c3");
            service.FillOutlinesForObjects(new[] { c3 }, "v");

            Assert.NotNull(c3.Outlines);
            Assert.Equal(4, c3.Outlines.Count);

            var outlineStrings = c3.Outlines.Select(o => o.ToString()).ToList();
            Assert.Contains("v/v1/v2/*c1/c2/c3", outlineStrings);
            Assert.Contains("v/v1/v2/*c2/c3", outlineStrings);
            Assert.Contains("v/v1/v2/*c3", outlineStrings);
            Assert.Contains("v/*c3", outlineStrings);

            //var allOutlineItems = category.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetCategories_When_NoCatalog_Expect_PhysicalAndVirtualOutlines()
        {
            var service = GetOutlineService();
            var c3 = GetTestData().OfType<Category>().FirstOrDefault(x => x.Id == "c3");
            service.FillOutlinesForObjects(new[] { c3 }, null);

            Assert.NotNull(c3.Outlines);
            Assert.Equal(5, c3.Outlines.Count);

            var outlineStrings = c3.Outlines.Select(o => o.ToString()).ToList();
            Assert.Contains("c/c1/c2/c3", outlineStrings);
            Assert.Contains("v/v1/v2/*c1/c2/c3", outlineStrings);
            Assert.Contains("v/v1/v2/*c2/c3", outlineStrings);
            Assert.Contains("v/v1/v2/*c3", outlineStrings);
            Assert.Contains("v/*c3", outlineStrings);

            //var allOutlineItems = category.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetProducts_When_NoCategoriesAndNoLinks_Expect_SinglePhysicalOutline()
        {
            var service = GetOutlineService();
            var p0 = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == "p0");
            service.FillOutlinesForObjects(new[] { p0 }, null);

            Assert.NotNull(p0.Outlines);
            Assert.Equal(1, p0.Outlines.Count);

            var outline = p0.Outlines.First();
            Assert.NotNull(outline.Items);
            Assert.Equal(2, outline.Items.Count);

            var outlineString = outline.ToString();
            Assert.Equal("c/p0", outlineString);

            //var allOutlineItems = p0.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetProducts_When_PhysicalCatalog_Expect_SinglePhysicalOutline()
        {
            var service = GetOutlineService();
            var p1 = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == "p1");
            service.FillOutlinesForObjects(new[] { p1 }, "c");


            Assert.NotNull(p1.Outlines);
            Assert.Equal(1, p1.Outlines.Count);

            var outline = p1.Outlines.First();
            Assert.NotNull(outline.Items);
            Assert.Equal(5, outline.Items.Count);

            var outlineString = outline.ToString();
            Assert.Equal("c/c1/c2/c3/p1", outlineString);

            //var allOutlineItems = p1.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetProducts_When_VirtualCatalog_Expect_MultipleVirtualOutlines()
        {
            var service = GetOutlineService();
            var p1 = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == "p1");
            service.FillOutlinesForObjects(new[] { p1 }, "v");

            Assert.NotNull(p1.Outlines);
            Assert.Equal(6, p1.Outlines.Count);

            var outlineStrings = p1.Outlines.Select(o => o.ToString()).ToList();
            Assert.Contains("v/v1/v2/*c1/c2/c3/p1", outlineStrings);
            Assert.Contains("v/v1/v2/*c2/c3/p1", outlineStrings);
            Assert.Contains("v/v1/v2/*c3/p1", outlineStrings);
            Assert.Contains("v/*c3/p1", outlineStrings);
            Assert.Contains("v/v1/v2/*p1", outlineStrings);
            Assert.Contains("v/*p1", outlineStrings);

            //var allOutlineItems = p1.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Theory]
        [InlineData("p1")]
        [InlineData("p1_v")]
        public void GetProductOrVariationOutlines_NoCatalog_PhysicalAndVirtualOutlines(string productId)
        {
            var service = GetOutlineService();
            var product = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == productId);
            service.FillOutlinesForObjects(new[] { product }, null);


            Assert.NotNull(product.Outlines);
            Assert.Equal(7, product.Outlines.Count);

            var outlineStrings = product.Outlines.Select(o => o.ToString()).ToList();
            Assert.Contains($"c/c1/c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c1/c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c3/{productId}", outlineStrings);
            Assert.Contains($"v/*c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*{productId}", outlineStrings);
            Assert.Contains($"v/*{productId}", outlineStrings);

            //var allOutlineItems = p1.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetVariationOutlines_VariationLinkedDirectlyToCategory_PhysicalAndVirtualOutlines()
        {
            var productId = "p1_v_linked";
            var service = GetOutlineService();
            var product = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == productId);
            service.FillOutlinesForObjects(new[] { product }, null);

            Assert.NotNull(product.Outlines);
            Assert.Equal(8, product.Outlines.Count);

            var outlineStrings = product.Outlines.Select(o => o.ToString()).ToList();
            Assert.Contains($"c/c1/c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c1/c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c2/c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*c3/{productId}", outlineStrings);
            Assert.Contains($"v/*c3/{productId}", outlineStrings);
            Assert.Contains($"v/v1/v2/*{productId}", outlineStrings);
            Assert.Contains($"v/*{productId}", outlineStrings);
            // Variation link outline is here
            Assert.Contains($"v/v1/*{productId}", outlineStrings);
        }

        private static IOutlineService GetOutlineService()
        {
            return new OutlineService(Mock.Of<IOutlinePartNameResolver>());
        }

        private static IEnumerable<IHasOutlines> GetTestData()
        {
            var c = new Catalog { Id = "c" };
            var v = new Catalog { Id = "v", IsVirtual = true };
            var c0 = new Category { CatalogId = c.Id, Catalog = c, Id = "c0" };
            var c1 = new Category { CatalogId = c.Id, Catalog = c, Id = "c1" };
            var c2 = new Category { CatalogId = c.Id, Catalog = c, Id = "c2", ParentId = c1.Id, Parents = new[] { c1 } };
            var c3 = new Category { CatalogId = c.Id, Catalog = c, Id = "c3", ParentId = c2.Id, Parents = new[] { c1, c2 } };
            var v1 = new Category { CatalogId = v.Id, Catalog = v, Id = "v1", IsVirtual = true };
            var v2 = new Category { CatalogId = v.Id, Catalog = v, Id = "v2", IsVirtual = true, ParentId = v1.Id, Parents = new[] { v1 } };
            var p0 = new CatalogProduct { CatalogId = c.Id, Id = "p0", Catalog = c };
            var p1 = new CatalogProduct { CatalogId = c.Id, Id = "p1", Catalog = c, CategoryId = c3.Id, Category = c3 };
            var p1_variation = new CatalogProduct { CatalogId = c.Id, Id = "p1_v", Catalog = c, CategoryId = c3.Id, Category = c3, MainProductId = "p1", MainProduct = p1 };
            var p1_variation_withLinks = new CatalogProduct { CatalogId = c.Id, Id = "p1_v_linked", Catalog = c, CategoryId = c3.Id, Category = c3, MainProductId = "p1", MainProduct = p1 };

            c1.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
            c2.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
            c3.Links = new[] {
                            new CategoryLink { CatalogId = v.Id, Catalog = v },
                            new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 },
                         };
            p1.Links = new[] {
                                 new CategoryLink { CatalogId = v.Id, Catalog = v },
                                 new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 },
            };
            p1_variation_withLinks.Links = new[] {
                                 new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v1.Id, Category = v1 },
            };

            return new IHasOutlines[] { c0, c1, c2, c3, v1, v2, p0, p1, p1_variation, p1_variation_withLinks };
        }
    }
}
