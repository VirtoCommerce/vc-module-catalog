using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Seo;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogSeoResolverTests
    {
        private const string CatalogProductType = "CatalogProduct";
        private const string CategoryType = "Category";

        public CatalogSeoResolverTests()
        {
        }

        [Fact]
        public async Task FindSeoAsync_ProductWithoutConflict_ReturnsSingleSeoInfo()
        {
            // Setup
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_TwoProductsWithConflictsAtLevel2_ReturnsSingleSeoInfo()
        {
            // Setup
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1-wrong", CatalogProductType, "product", true, "B2B-store", "en-US");

            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", CategoryType, "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level2-wrong", CategoryType, "level2-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level1", CategoryType, "level1", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_TwoCategoriesWithConflictsAtLevel3_ReturnsSingleSeoInfo()
        {
            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/category", StoreId = "B2B-store", LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", CatalogProductType, "level3-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category", CategoryType, "category", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category-wrong", CategoryType, "category", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_NonExistentPermalink_ReturnsEmptyList()
        {
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", CatalogProductType, "level3", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_TwoProductsWithConflictsAtLevel3_ReturnsSingleSeoInfo()
        {
            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = "B2B-store", LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1-wrong", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_TwoCategoriesWithSameSlugAtLevel2_ReturnsSingleSeoInfo()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("level3", CategoryType, "level3", true, "B2B-store", "en-US");
            helper.AddSeoInfo("level3-wrong", CategoryType, "level3-wrong", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category", CategoryType, "category", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category-wrong", CategoryType, "category", true, "B2B-store", "en-US");

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
        public async Task FindSeoAsync_SeoInfoForCurrentStoreOnly_ReturnsSingleSeoInfo()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1", CatalogProductType, "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", CatalogProductType, "product", true, "Other-store", "en-US");

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
        public async Task FindSeoAsync_SeoInfoForNullStore_ReturnsSingleSeoInfo()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("product1", CatalogProductType, "product", true, "B2B-store", "en-US");
            helper.AddSeoInfo("product1", CatalogProductType, "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", CatalogProductType, "product", true, "Other-store", "en-US");

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
        public async Task FindSeoAsync_TwoCategoriesWithSameSlugInRoot_ReturnsEmptyList()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper();

            helper.AddSeoInfo("category1", CategoryType, "category", true, "B2B-store", "en-US");
            helper.AddSeoInfo("category2", CategoryType, "category", true, "B2B-store", "en-US");

            helper.AddCategory("category1", "category");
            helper.AddCategory("category2", "category");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "category", StoreId = "B2B-store", LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Empty(result);
        }


    }
}

