using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Seo;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogSeoResolverTests
    {
        private const string CatalogId = "CatalogId";
        private const string ProductType = "CatalogProduct";
        private const string CategoryType = "Category";

        private const string StoreId = "B2B-store";

        public CatalogSeoResolverTests()
        {
        }

        [Fact]
        public async Task FindSeoAsync_ProductWithoutConflict_ReturnsSingleSeoInfo()
        {
            // Setup
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("category", $"{CatalogId}/level1", $"{CatalogId}/level1/level2", $"{CatalogId}/level1/level2/level3");
            helper.AddCategory("category-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = "en-US" };

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
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product1-wrong", ProductType, "product", true, StoreId, "en-US");

            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");
            helper.AddSeoInfo("level3-wrong", CategoryType, "level3", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
            helper.AddSeoInfo("level2-wrong", CategoryType, "level2-wrong", true, StoreId, "en-US");
            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level2-wrong", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1/level2");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1/level2-wrong");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2-wrong/level3");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = "en-US" };

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
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/category", StoreId = StoreId, LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");
            helper.AddSeoInfo("level3-wrong", ProductType, "level3-wrong", true, StoreId, "en-US");
            helper.AddSeoInfo("category", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category-wrong", CategoryType, "category", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1/level");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1/level2");
            helper.AddCategory("category", $"{CatalogId}/level1/level2/level3");
            helper.AddCategory("category-wrong", $"{CatalogId}/level1/level2/level3-wrong");

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
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");
            helper.AddSeoInfo("level3-wrong", ProductType, "level3", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("category", $"{CatalogId}/level1", $"{CatalogId}/level1/level2", $"{CatalogId}/level1/level2/level3");
            helper.AddCategory("category-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            // Arrange
            var criteria = new SeoSearchCriteria { Permalink = "non-existent", StoreId = StoreId, LanguageCode = "en-US" };

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
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = "en-US" };

            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product1-wrong", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

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
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");
            helper.AddSeoInfo("level3-wrong", CategoryType, "level3-wrong", true, StoreId, "en-US");
            helper.AddSeoInfo("category", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category-wrong", CategoryType, "category", true, StoreId, "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1", $"{CatalogId}/level1/level2");
            helper.AddCategory("level3-wrong", $"{CatalogId}/level1/level2");
            helper.AddCategory("category", $"{CatalogId}/level1/level2/level3");
            helper.AddCategory("category-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/category", StoreId = StoreId, LanguageCode = "en-US" };
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
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product1", ProductType, "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", ProductType, "product", true, "Other-store", "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1/level2");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
            Assert.Equal(StoreId, result.First().StoreId);
        }

        [Fact]
        public async Task FindSeoAsync_SeoInfoForNullStore_ReturnsSingleSeoInfo()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product1", ProductType, "product", true, null, "en-US");
            helper.AddSeoInfo("product1-wrong", ProductType, "product", true, "Other-store", "en-US");

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1/level2");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("product1", result.First().ObjectId);
            Assert.Equal(StoreId, result.First().StoreId);
        }

        [Fact]
        public async Task FindSeoAsync_TwoCategoriesWithSameSlugInRoot_ReturnsEmptyList()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("category1", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category2", CategoryType, "category", true, StoreId, "en-US");

            helper.AddCategory("category1", $"{CatalogId}/category");
            helper.AddCategory("category2", $"{CatalogId}/category");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "category", StoreId = StoreId, LanguageCode = "en-US" };

            // Act
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public Task Wrong_ProductId_ErrorHandling()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product2", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("category1", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category2", CategoryType, "category2", true, StoreId, "en-US");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "category/product", StoreId = StoreId, LanguageCode = "en-US" };

            // Act & Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => seoResolver.FindSeoAsync(criteria));
        }

        [Fact]
        public Task Wrong_CategoryId_ErrorHandling()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("category11", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category21", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("category2", CategoryType, "level1-wrong", true, StoreId, "en-US");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/category", StoreId = StoreId, LanguageCode = "en-US" };

            // Act & Assert
            return Assert.ThrowsAsync<InvalidOperationException>(() => seoResolver.FindSeoAsync(criteria));
        }

        [Fact]

        public async Task FindSeoAsync_Same_As_Root()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("category", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category-child", CategoryType, "category", true, StoreId, "en-US");

            helper.AddCategory("category", $"{CatalogId}");
            helper.AddCategory("category-child", $"{CatalogId}/category");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "category", StoreId = StoreId, LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("category", result.First().ObjectId);

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "category/category", StoreId = StoreId, LanguageCode = "en-US" };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // Assert
            Assert.Single(result2);
            Assert.Equal("category-child", result2.First().ObjectId);
        }

        [Fact]
        public async Task FindSeoAsync_Education()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            // Education
            helper.AddCategory("EducationCategoryId", CatalogId);

            // Furniture & Furnishings
            helper.AddCategory("FurnitureCategoryId", CatalogId);

            // Furniture & Furnishings > Education
            helper.AddCategory("EducationInFurnitureCategoryId", CatalogId, $"{CatalogId}/FurnitureCategoryId");


            helper.AddSeoInfo("EducationCategoryId", CategoryType, "education", true, null, null);
            helper.AddSeoInfo("FurnitureCategoryId", CategoryType, "furniture-furnishings", true, string.Empty, string.Empty);
            helper.AddSeoInfo("EducationInFurnitureCategoryId", CategoryType, "education", true, StoreId, string.Empty);


            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "education", StoreId = StoreId, LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("EducationCategoryId", result.First().ObjectId);

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "furniture-furnishings/education", StoreId = StoreId, LanguageCode = "en-US" };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // Assert
            Assert.Single(result2);
            Assert.Equal("EducationInFurnitureCategoryId", result2.First().ObjectId);
        }
    }
}

