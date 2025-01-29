using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Seo;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogSeoResolverTests
    {
        public CatalogSeoResolverTests()
        {
        }



        [Fact]
        public async Task Resolve_Product_Without_Conflict()
        {
            // Setup
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level3-wrong", "level1/level2/level3-wrong");
            helper.AddCategory("category", "level1/category", "level1/level2/category", "level1/level2/level3/category");
            helper.AddCategory("category-wrong", "level1/level2/level3-wrong/category-wrong");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2/level3-wrong/product1-wrong");


            var seoResolver = helper.CreateCatalogSeoResolver();

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
        }

        [Fact]
        public async Task Resolve_From_TwoProducts_Conflicts_Level2()
        {
            // Setup
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1-wrong", "CatalogProduct", "product", true, "B2B-store", "en-US");

            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", "Category", "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level2", "Category", "level2", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level2-wrong", "Category", "level2-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level1", "Category", "level1", true, "B2B-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level2-wrong", "level1/level2-wrong");
            helper.AddCategory("level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level2-wrong/level3");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2-wrong/level3/product1-wrong");


            var seoResolver = helper.CreateCatalogSeoResolver();

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
        }

        [Fact]
        public async Task Resolve_From_TwoCategories_Conflicts_Level3()
        {
            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/category", StoreId = "B2B-store", LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", "CatalogProduct", "level3-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category", "Category", "category", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category-wrong", "Category", "category", true, "B2B-store", "en-US");


            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level2/level3-wrong");
            helper.AddCategory("category", "level1/level2/level3/category");
            helper.AddCategory("category-wrong", "level1/level2/level3-wrong/category-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("category", result.First().ObjectId);
        }

        [Fact]
        public async Task Empty_Response()
        {
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", "CatalogProduct", "level3", true, "B2B-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level3-wrong", "level1/level2/level3-wrong");
            helper.AddCategory("category", "level1/category", "level1/level2/category", "level1/level2/level3/category");
            helper.AddCategory("category-wrong", "level1/level2/level3-wrong/category-wrong");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2/level3-wrong/product1-wrong");

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "non-existent", StoreId = "B2B-store", LanguageCode = "en-US" };

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Resolve_From_TwoProructs_Conflicts_Level3()
        {
            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1-wrong", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level3-wrong", "level1/level2/level3-wrong");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2/level3-wrong/product1-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
        }

        [Fact]
        public async Task TwoProductsWithSameSlug_Level2()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", "Category", "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", "Category", "level3-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category", "Category", "category", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category-wrong", "Category", "category", true, "B2B-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level3", "level1/level2/level3");
            helper.AddCategory("level3-wrong", "level1/level2/level3-wrong");
            helper.AddCategory("category", "level1/level2/level3/category");
            helper.AddCategory("category-wrong", "level1/level2/level3-wrong/category-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/category", StoreId = "B2B-store", LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("category", result.First().ObjectId);
            Assert.Equal("Category", result.First().ObjectType);
        }
        [Fact]
        public async Task Resolve_SeoInfo_For_Current_Store_Only()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", "CatalogProduct", "product", true, "Other-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level2/level3");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2/level3-wrong/product1-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
            Assert.Equal("B2B-store", result.First().StoreId);
        }

        [Fact]
        public async Task Resolve_SeoInfo_For_Null_Store()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1", "CatalogProduct", "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", "CatalogProduct", "product", true, "Other-store", "en-US");

            helper.AddCategory("level1", "level1");
            helper.AddCategory("level2", "level1/level2");
            helper.AddCategory("level3", "level1/level2/level3");

            helper.AddProduct("product1", "level1/level2/level3/product1");
            helper.AddProduct("product1-wrong", "level1/level2/level3-wrong/product1-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
            Assert.Equal("B2B-store", result.First().StoreId);
        }
    }
}

