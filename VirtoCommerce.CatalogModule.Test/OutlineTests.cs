using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using Xunit;


namespace VirtoCommerce.CatalogModule.Test
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
            Assert.True(outlineStrings.Contains("v/v1/v2/*c1/c2/c3"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c2/c3"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c3"));
            Assert.True(outlineStrings.Contains("v/*c3"));

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
            Assert.True(outlineStrings.Contains("c/c1/c2/c3"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c1/c2/c3"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c2/c3"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c3"));
            Assert.True(outlineStrings.Contains("v/*c3"));

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
            Assert.True(outlineStrings.Contains("v/v1/v2/*c1/c2/c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c2/c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c3/p1"));
            Assert.True(outlineStrings.Contains("v/*c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*p1"));
            Assert.True(outlineStrings.Contains("v/*p1"));

            //var allOutlineItems = p1.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        [Fact]
        public void GetProducts_When_NoCatalog_Expect_PhysicalAndVirtualOutlines()
        {
            var service = GetOutlineService();
            var p1 = GetTestData().OfType<CatalogProduct>().FirstOrDefault(x => x.Id == "p1");
            service.FillOutlinesForObjects(new[] { p1 }, null);


            Assert.NotNull(p1.Outlines);
            Assert.Equal(7, p1.Outlines.Count);

            var outlineStrings = p1.Outlines.Select(o => o.ToString()).ToList();
            Assert.True(outlineStrings.Contains("c/c1/c2/c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c1/c2/c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c2/c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*c3/p1"));
            Assert.True(outlineStrings.Contains("v/*c3/p1"));
            Assert.True(outlineStrings.Contains("v/v1/v2/*p1"));
            Assert.True(outlineStrings.Contains("v/*p1"));

            //var allOutlineItems = p1.Outlines.SelectMany(o => o.Items.Where(i => i.SeoObjectType != "Catalog")).ToList();
            //Assert.True(allOutlineItems.All(i => i.SeoInfos != null && i.SeoInfos.Any()));
        }

        private static IOutlineService GetOutlineService()
        {
            return new OutlineService();
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

            return new IHasOutlines[] { c0, c1, c2, c3, v1, v2, p0, p1 };
        }


    }
}
