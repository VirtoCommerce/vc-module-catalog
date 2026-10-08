using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Models.Explain;
using Xunit;
using CatalogReasons = VirtoCommerce.CatalogModule.Core.ModuleConstants.SeoCandidateReasons;
using SeoReasons = VirtoCommerce.Seo.Core.ModuleConstants.CandidateReasons;

namespace VirtoCommerce.CatalogModule.Tests
{
    /// <summary>
    /// GetCandidatesAsync explains FindSeoAsync: it returns the records FindSeoAsync resolves, plus the records
    /// with the same slug that it rejects, each with the reasons. Every test also checks that the resolved
    /// candidates are exactly what FindSeoAsync returns.
    /// </summary>
    public class CatalogSeoResolverCandidatesTests
    {
        private const string CatalogId = "CatalogId";
        private const string ProductType = "CatalogProduct";
        private const string CategoryType = "Category";

        private const string StoreId = "B2B-store";
        private const string LanguageCode = "en-US";

        [Fact]
        public async Task GetCandidatesAsync_CategoryNotInStoreCatalog_ReportsNotInStoreCatalog()
        {
            // VCST-5989: "v-dresses" belongs to a category of another catalog. Its SEO record exists and isn't
            // restricted to a store, but the category service returns no outline for the store's catalog,
            // so the store can't resolve it. Before, explain returned nothing, exactly as for an unknown slug.
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo("dresses", CategoryType, "v-dresses", true, null, null);
            helper.AddCategory("dresses");

            var criteria = new SeoSearchCriteria { Permalink = "v-dresses", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            var candidate = Assert.Single(candidates);
            Assert.Equal("dresses", candidate.SeoInfo.ObjectId);
            Assert.Equal([CatalogReasons.NotInStoreCatalog], ReasonCodes(candidate));
        }

        [Fact]
        public async Task GetCandidatesAsync_UnknownSlug_ReturnsNothing()
        {
            // Control for the case above: a slug without any SEO record has no candidates at all.
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo("dresses", CategoryType, "v-dresses", true, null, null);
            helper.AddCategory("dresses", CatalogId);

            var criteria = new SeoSearchCriteria { Permalink = "no-such-slug-at-all", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Empty(candidates);
        }

        [Fact]
        public async Task GetCandidatesAsync_RecordsOfOneProductAndOfAnotherStore_ReportsEveryReason()
        {
            // VP-9295 shape: the same slug has a store-specific record, a global record and a record assigned to another store.
            // - the store-specific record wins;
            // - the global record of the same product loses to it (NotBestMatch);
            // - the other store's record fails the store filter AND can't get a path in this store, both reasons are reported.
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("product1", ProductType, "product", true, StoreId, LanguageCode);
            helper.AddSeoInfo("product1", ProductType, "product", true, null, LanguageCode);
            helper.AddSeoInfo("product1-wrong", ProductType, "product", true, "Other-store", LanguageCode);

            helper.AddSeoInfo("level1", CategoryType, "level1", true, StoreId, LanguageCode);
            helper.AddSeoInfo("level2", CategoryType, "level2", true, StoreId, LanguageCode);
            helper.AddSeoInfo("level3", CategoryType, "level3", true, StoreId, LanguageCode);

            helper.AddCategory("level1", $"{CatalogId}");
            helper.AddCategory("level2", $"{CatalogId}/level1");
            helper.AddCategory("level3", $"{CatalogId}/level1/level2");

            helper.AddProduct("product1", $"{CatalogId}/level1/level2/level3");
            helper.AddProduct("product1-wrong", $"{CatalogId}/level1/level2/level3-wrong");

            var criteria = new SeoSearchCriteria { Permalink = "level1/level2/level3/product", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Equal(3, candidates.Count);

            var resolved = Assert.Single(candidates, x => x.IsResolved);
            Assert.Equal("product1", resolved.SeoInfo.ObjectId);
            Assert.Equal(StoreId, resolved.SeoInfo.StoreId);

            var globalRecord = Assert.Single(candidates, x => x.SeoInfo.ObjectId == "product1" && x.SeoInfo.StoreId == null);
            Assert.Equal([SeoReasons.NotBestMatch], ReasonCodes(globalRecord));

            var otherStoreRecord = Assert.Single(candidates, x => x.SeoInfo.ObjectId == "product1-wrong");
            Assert.Equal([SeoReasons.StoreMismatch, CatalogReasons.NoSeoPath], ReasonCodes(otherStoreRecord));
        }

        [Fact]
        public async Task GetCandidatesAsync_ObjectWithAnotherPath_ReportsTheStorePath()
        {
            // Two categories share the slug "category": the root one resolves, the child one is reachable only
            // as "category/category". The mismatch carries the store's path, which is the actual diagnosis.
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("category", CategoryType, "category", true, StoreId, LanguageCode);
            helper.AddSeoInfo("category-child", CategoryType, "category", true, StoreId, LanguageCode);

            helper.AddCategory("category", $"{CatalogId}");
            helper.AddCategory("category-child", $"{CatalogId}/category");

            var criteria = new SeoSearchCriteria { Permalink = "category", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Equal("category", Assert.Single(candidates, x => x.IsResolved).SeoInfo.ObjectId);

            var child = Assert.Single(candidates, x => x.SeoInfo.ObjectId == "category-child");
            var reason = Assert.Single(child.Reasons);
            Assert.Equal(CatalogReasons.PermalinkMismatch, reason.Code);
            Assert.Equal("category/category", reason.Details);
        }

        [Fact]
        public async Task GetCandidatesAsync_RecordsFailingFilters_ReportsEachFilter()
        {
            // One reason per SQL filter: an inactive record, a record in another language and a record of an inactive category.
            // The other-language record also has no slug in the requested language, so the store can't build its path.
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("inactive-record", CategoryType, "slug", false, null, LanguageCode);
            helper.AddSeoInfo("other-language", CategoryType, "slug", true, null, "de-DE");
            helper.AddSeoInfo("inactive-category", CategoryType, "slug", true, null, LanguageCode);

            helper.AddCategory("inactive-record", CatalogId);
            helper.AddCategory("other-language", CatalogId);
            helper.AddCategory("inactive-category", CatalogId);
            helper.Categories.Last().IsActive = false;

            var criteria = new SeoSearchCriteria { Permalink = "slug", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Equal(3, candidates.Count);
            Assert.Equal([SeoReasons.Inactive], ReasonCodes(candidates.Single(x => x.SeoInfo.ObjectId == "inactive-record")));
            Assert.Equal([SeoReasons.LanguageMismatch, CatalogReasons.NoSeoPath], ReasonCodes(candidates.Single(x => x.SeoInfo.ObjectId == "other-language")));
            Assert.Equal([CatalogReasons.ObjectInactive], ReasonCodes(candidates.Single(x => x.SeoInfo.ObjectId == "inactive-category")));
        }

        [Fact]
        public async Task GetCandidatesAsync_SamePathUnderAnotherParent_ReportsParentMismatch()
        {
            // "education" is the path of a root category and, through a second outline, of a category that lives
            // under Furniture. Both paths match, so the parent check decides: the one whose longest outline isn't
            // under the catalog root is misplaced.
            // The root category's record scores higher, so it is checked first and selected. FindSeoAsync stops there;
            // only explain goes on to check the misplaced one, so only it can tell ParentMismatch from NotBestMatch.
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("EducationCategoryId", CategoryType, "education", true, StoreId, LanguageCode);
            helper.AddSeoInfo("FurnitureCategoryId", CategoryType, "furniture-furnishings", true, string.Empty, string.Empty);
            helper.AddSeoInfo("EducationInFurnitureCategoryId", CategoryType, "education", true, null, null);

            helper.AddCategory("EducationCategoryId", CatalogId);
            helper.AddCategory("FurnitureCategoryId", CatalogId);
            helper.AddCategory("EducationInFurnitureCategoryId", CatalogId, $"{CatalogId}/FurnitureCategoryId");

            var criteria = new SeoSearchCriteria { Permalink = "education", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Equal("EducationCategoryId", Assert.Single(candidates, x => x.IsResolved).SeoInfo.ObjectId);
            Assert.Equal([CatalogReasons.ParentMismatch], ReasonCodes(candidates.Single(x => x.SeoInfo.ObjectId == "EducationInFurnitureCategoryId")));
        }

        [Fact]
        public async Task GetCandidatesAsync_TwoObjectsPassingTheParentCheck_ReportsTheLoserAsNotBestMatch()
        {
            // Two root categories with the same slug both pass every check. FindSeoAsync stops at the first one;
            // explain checks the second one too and reports that it lost, not that it is misplaced.
            var helper = new CatalogHierarchyHelper(CatalogId);

            helper.AddSeoInfo("first", CategoryType, "duplicate", true, StoreId, LanguageCode);
            helper.AddSeoInfo("second", CategoryType, "duplicate", true, StoreId, LanguageCode);

            helper.AddCategory("first", CatalogId);
            helper.AddCategory("second", CatalogId);

            var criteria = new SeoSearchCriteria { Permalink = "duplicate", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(helper.CreateCatalogSeoResolver(), criteria);

            Assert.Single(candidates, x => x.IsResolved);
            Assert.Equal([SeoReasons.NotBestMatch], ReasonCodes(Assert.Single(candidates, x => !x.IsResolved)));
        }

        [Fact]
        public async Task GetCandidatesAsync_MoreRecordsThanTake_ListsThisStoresRecordsFirst()
        {
            // Explain lists at most criteria.Take rejected records. When many stores share a slug (VP-9295: 150+ stores,
            // a record per store), the records this store could use must come first, or the limit would hide them:
            // this store's or store-less records first, then the requested language, then Id, so the order never changes.
            // The ids are chosen so that Id order alone would put the other stores' records first.
            var helper = new CatalogHierarchyHelper(CatalogId);

            AddRejectedRecord("a1", "other-store-1", "Other-store", LanguageCode);
            AddRejectedRecord("a2", "other-store-2", "Other-store", LanguageCode);
            AddRejectedRecord("b", "this-store-other-language", StoreId, "de-DE");
            AddRejectedRecord("c", "this-store", StoreId, LanguageCode);

            var criteria = new SeoSearchCriteria { Permalink = "slug", StoreId = StoreId, LanguageCode = LanguageCode, Take = 3 };
            var resolver = helper.CreateCatalogSeoResolver();

            var candidates = await AssertCandidatesExplainFindSeoAsync(resolver, criteria);
            var candidatesAgain = await resolver.GetCandidatesAsync(criteria);

            Assert.Equal(["this-store", "this-store-other-language", "other-store-1"], candidates.Select(x => x.SeoInfo.ObjectId));
            Assert.Equal(candidates.Select(x => x.SeoInfo.Id), candidatesAgain.Select(x => x.SeoInfo.Id));

            // Categories without outlines are outside the store catalog, so every record is rejected
            void AddRejectedRecord(string id, string categoryId, string storeId, string languageCode)
            {
                helper.AddSeoInfo(categoryId, CategoryType, "slug", true, storeId, languageCode);
                helper.SeoInfos[^1].Id = id;
                helper.AddCategory(categoryId);
            }
        }

        [Fact]
        public async Task GetCandidatesAsync_SubclassOverridingFindSeoAsync_FollowsItsResult()
        {
            // A project may subclass CatalogSeoResolver and override only FindSeoAsync, the historical extension point.
            // Explain must follow the override: a record the override drops is rejected, a record it adds is resolved.
            var helper = new CatalogHierarchyHelper(CatalogId);
            helper.AddSeoInfo("hidden", CategoryType, "slug", true, StoreId, LanguageCode);
            helper.AddCategory("hidden", CatalogId);

            var extra = new SeoInfo { Id = "extra", ObjectId = "extra", ObjectType = CategoryType, SemanticUrl = "slug" };
            var resolver = new OverridingResolver(helper, hiddenObjectId: "hidden", extra);

            var criteria = new SeoSearchCriteria { Permalink = "slug", StoreId = StoreId, LanguageCode = LanguageCode };

            var candidates = await AssertCandidatesExplainFindSeoAsync(resolver, criteria);

            Assert.Equal([SeoReasons.NotReturnedByResolver], ReasonCodes(candidates.Single(x => x.SeoInfo.ObjectId == "hidden")));
            Assert.True(candidates.Single(x => x.SeoInfo.ObjectId == "extra").IsResolved);
        }

        /// <summary>
        /// The invariant behind explain: the resolved candidates are FindSeoAsync's result, in the same order.
        /// </summary>
        private static async Task<IList<SeoExplainItem>> AssertCandidatesExplainFindSeoAsync(CatalogSeoResolver resolver, SeoSearchCriteria criteria)
        {
            var seoInfos = await resolver.FindSeoAsync(criteria);
            var candidates = await resolver.GetCandidatesAsync(criteria);

            Assert.Equal(
                seoInfos.Select(x => x.Id),
                candidates.Where(x => x.IsResolved).Select(x => x.SeoInfo.Id));

            return candidates;
        }

        private static string[] ReasonCodes(SeoExplainItem candidate)
        {
            return candidate.Reasons.Select(x => x.Code).ToArray();
        }

        /// <summary>
        /// A project's resolver that hides one object and adds a record of its own.
        /// </summary>
        private sealed class OverridingResolver(CatalogHierarchyHelper helper, string hiddenObjectId, SeoInfo extra)
            : CatalogSeoResolver(
                helper.CreateCatalogRepositoryMock().Object,
                helper.CreateCategoryServiceMock().Object,
                helper.CreateProductServiceMock().Object,
                helper.CreateStoreServiceMock().Object)
        {
            public override async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
            {
                var seoInfos = await base.FindSeoAsync(criteria);
                return seoInfos.Where(x => x.ObjectId != hiddenObjectId).Append(extra).ToList();
            }
        }
    }
}
