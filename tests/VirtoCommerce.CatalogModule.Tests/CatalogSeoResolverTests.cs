using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Seo.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogSeoResolverTests
    {
        private const string CatalogId = "CatalogId";
        private const string ProductType = "CatalogProduct";
        private const string CategoryType = "Category";
        private const string CatalogType = "Catalog";

        private const string StoreId = "B2B-store";
        private const string LanguageCode = "en-US";
        private const string AlternativeLanguageCode = "de-DE";

        public CatalogSeoResolverTests()
        {
        }

        [Fact]
        public async Task FindSeoAsync_ProductWithoutConflict_ReturnsSingleSeoInfo()
        {
            // Setup
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
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

            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
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
            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
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

            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
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

            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");

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

            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, "en-US");
            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, "en-US");

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
        public async Task Wrong_ProductId_ErrorHandling()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("product2", ProductType, "product", true, StoreId, "en-US");
            helper.AddSeoInfo("category1", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category2", CategoryType, "category2", true, StoreId, "en-US");

            helper.AddProduct("product1", $"{CatalogId}");
            helper.AddProduct("product2", $"{CatalogId}");
            helper.AddCategory("category1", $"{CatalogId}");
            helper.AddCategory("category2", $"{CatalogId}");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "category/product", StoreId = StoreId, LanguageCode = "en-US" };

            // Act & Assert
            var result = await seoResolver.FindSeoAsync(criteria);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Wrong_CategoryId_ErrorHandling()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("category11", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category21", CategoryType, "category", true, StoreId, "en-US");
            helper.AddSeoInfo("category1", CategoryType, "level1", true, StoreId, "en-US");
            helper.AddSeoInfo("category2", CategoryType, "level1-wrong", true, StoreId, "en-US");

            helper.AddCategory("category11", $"{CatalogId}");
            helper.AddCategory("category21", $"{CatalogId}");
            helper.AddCategory("category1", $"{CatalogId}");
            helper.AddCategory("category2", $"{CatalogId}");

            var seoResolver = helper.CreateCatalogSeoResolver();

            var criteria = new SeoSearchCriteria { Permalink = "level1/category", StoreId = StoreId, LanguageCode = "en-US" };

            // Act & Assert
            // Act & Assert
            var result = await seoResolver.FindSeoAsync(criteria);
            Assert.Empty(result);
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
        public async Task FindSeoAsync_Education_LongLinks()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("EducationCategoryId", CategoryType, "education", true, null, null);
            helper.AddSeoInfo("FurnitureCategoryId", CategoryType, "furniture-furnishings", true, string.Empty, string.Empty);
            helper.AddSeoInfo("EducationInFurnitureCategoryId", CategoryType, "education", true, StoreId, string.Empty);

            // Education
            helper.AddCategory("EducationCategoryId", CatalogId);

            // Furniture & Furnishings
            helper.AddCategory("FurnitureCategoryId", CatalogId);

            // Furniture & Furnishings > Education
            helper.AddCategory("EducationInFurnitureCategoryId", CatalogId, $"{CatalogId}/FurnitureCategoryId");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria1 = new SeoSearchCriteria { Permalink = "education", StoreId = StoreId, LanguageCode = "en-US" };
            var result1 = await seoResolver.FindSeoAsync(criteria1);

            // Assert
            Assert.Single(result1);
            Assert.Equal("EducationCategoryId", result1.First().ObjectId);

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "furniture-furnishings/education", StoreId = StoreId, LanguageCode = "en-US" };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // Assert
            Assert.Single(result2);
            Assert.Equal("EducationInFurnitureCategoryId", result2.First().ObjectId);
        }

        [Fact]
        public async Task FindSeoAsync_Education_ShortLinks()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId, StoreModule.Core.ModuleConstants.Settings.SEO.SeoShort);

            helper.AddSeoInfo("EducationCategoryId", CategoryType, "education", isActive: true, storeId: null, languageCode: null);
            helper.AddSeoInfo("FurnitureCategoryId", CategoryType, "furniture-furnishings", isActive: true, storeId: string.Empty, languageCode: string.Empty);
            helper.AddSeoInfo("EducationInFurnitureCategoryId", CategoryType, "education", isActive: true, StoreId, languageCode: string.Empty);

            // Education
            helper.AddCategory("EducationCategoryId", CatalogId);

            // Furniture & Furnishings
            helper.AddCategory("FurnitureCategoryId", CatalogId);

            // Furniture & Furnishings > Education
            helper.AddCategory("EducationInFurnitureCategoryId", CatalogId, $"{CatalogId}/FurnitureCategoryId");

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "education", StoreId = StoreId, LanguageCode = LanguageCode };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("EducationCategoryId", result.Select(x => x.ObjectId));
            Assert.Contains("EducationInFurnitureCategoryId", result.Select(x => x.ObjectId));

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "furniture-furnishings/education", StoreId = StoreId, LanguageCode = LanguageCode };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // Assert
            Assert.Empty(result2);
        }

        [Fact]
        public async Task FindSeoAsync_Catalog()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo(CatalogId, CatalogType, "catalog", true, StoreId, string.Empty);
            helper.AddSeoInfo("CatalogId+localized", CatalogType, "catalog", true, StoreId, "en-US");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "catalog", StoreId = StoreId, LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("catalog", result.First().SemanticUrl);
        }

        [Fact]
        public async Task FindSeoAsync_WithoutStore_ReturnsEmpty()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo(CatalogId, CatalogType, "catalog", isActive: true, StoreId, string.Empty);
            helper.AddSeoInfo("CatalogId1", CatalogType, "catalog", isActive: true, StoreId, "en-US");
            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "catalog", StoreId = null, LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FindSeoAsync_ReturnsBestMatchingRecord()
        {
            // Arrange
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo(CatalogId, CatalogType, "catalog", isActive: true, storeId: null, languageCode: string.Empty);
            helper.AddSeoInfo(CatalogId, CatalogType, "catalog", isActive: true, StoreId, languageCode: string.Empty);
            helper.AddSeoInfo(CatalogId, CatalogType, "catalog", isActive: true, storeId: null, "en-US");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();


            // Act
            var criteria = new SeoSearchCriteria { Permalink = "catalog", StoreId = StoreId, LanguageCode = "en-US" };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal(StoreId, result.First().StoreId);
        }

        [Fact]
        public async Task FindSeoAsync_FullUrlCorrect()
        {
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("CategoryId", CategoryType, "category", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId", CategoryType, "subcategory", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("ProductId", ProductType, "product", isActive: true, StoreId, LanguageCode);

            helper.AddCategory("CategoryId", CatalogId);
            helper.AddCategory("SubCategoryId", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddProduct("ProductId", CatalogId, "CategoryId", $"{CatalogId}/CategoryId/SubCategoryId");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "category/subcategory/product", StoreId = StoreId, LanguageCode = LanguageCode };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal(StoreId, result.First().StoreId);
            Assert.Equal("product", result.First().SemanticUrl);
        }

        [Fact]
        public async Task FindSeoAsync_FullUrlIncorrect()
        {
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("CategoryId", CategoryType, "category", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId", CategoryType, "subcategory", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("ProductId", ProductType, "product", isActive: true, StoreId, LanguageCode);

            helper.AddCategory("CategoryId", CatalogId);
            helper.AddCategory("SubCategoryId", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddProduct("ProductId", CatalogId, "CategoryId", $"{CatalogId}/CategoryId/SubCategoryId");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria = new SeoSearchCriteria { Permalink = "category/subcategory1/product", StoreId = StoreId, LanguageCode = LanguageCode };
            var result = await seoResolver.FindSeoAsync(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FindSeoAsync_SameUrlDifferentLanguage()
        {
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("CategoryId", CategoryType, "category", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId1", CategoryType, "subcategory", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId2", CategoryType, "subcategory", isActive: true, StoreId, AlternativeLanguageCode);
            helper.AddSeoInfo("SubCategoryId3", CategoryType, "subcategory3", isActive: true, StoreId, LanguageCode);

            helper.AddCategory("CategoryId", CatalogId);
            helper.AddCategory("SubCategoryId1", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddCategory("SubCategoryId2", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddCategory("SubCategoryId3", CatalogId, $"{CatalogId}/CategoryId");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria1 = new SeoSearchCriteria { Permalink = "category/subcategory", StoreId = StoreId, LanguageCode = LanguageCode };
            var result1 = await seoResolver.FindSeoAsync(criteria1);

            // Assert
            Assert.Single(result1);
            Assert.Equal(LanguageCode, result1.First().LanguageCode);

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "category/subcategory", StoreId = StoreId, LanguageCode = AlternativeLanguageCode };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // Assert
            Assert.Single(result2);
            Assert.Equal(AlternativeLanguageCode, result2.First().LanguageCode);
        }

        [Fact]
        public async Task FindSeoAsync_SameUrlDifferentLanguage_ShortLinks()
        {
            var helper = new CatalogHierarchyHelper(CatalogId, StoreModule.Core.ModuleConstants.Settings.SEO.SeoShort);

            helper.AddSeoInfo("CategoryId", CategoryType, "category", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId1", CategoryType, "subcategory", isActive: true, StoreId, LanguageCode);
            helper.AddSeoInfo("SubCategoryId2", CategoryType, "subcategory", isActive: true, StoreId, AlternativeLanguageCode);
            helper.AddSeoInfo("SubCategoryId3", CategoryType, "subcategory", isActive: true, StoreId, languageCode: "other-lang");
            helper.AddSeoInfo("SubCategoryId4", CategoryType, "subcategory3", isActive: true, StoreId, LanguageCode);

            helper.AddCategory("CategoryId", CatalogId);
            helper.AddCategory("SubCategoryId1", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddCategory("SubCategoryId2", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddCategory("SubCategoryId3", CatalogId, $"{CatalogId}/CategoryId");
            helper.AddCategory("SubCategoryId4", CatalogId, $"{CatalogId}/CategoryId");

            helper.AddCatalog(CatalogId);

            var seoResolver = helper.CreateCatalogSeoResolver();

            // Act
            var criteria1 = new SeoSearchCriteria { Permalink = "subcategory", StoreId = StoreId, LanguageCode = LanguageCode };
            var result1 = await seoResolver.FindSeoAsync(criteria1);

            // Assert
            Assert.Single(result1);
            Assert.Equal(LanguageCode, result1.First().LanguageCode);

            // Act
            var criteria2 = new SeoSearchCriteria { Permalink = "subcategory", StoreId = StoreId, LanguageCode = AlternativeLanguageCode };
            var result2 = await seoResolver.FindSeoAsync(criteria2);

            // have to find the both records

            // Assert
            Assert.Equal(2, result2.Count);
            Assert.Contains(AlternativeLanguageCode, result2.Select(x => x.LanguageCode));
            Assert.Contains(LanguageCode, result2.Select(x => x.LanguageCode));
        }
    }
}
