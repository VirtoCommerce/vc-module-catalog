using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.Extensions.SeoExtensions;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogHierarchyHelper
    {
        private readonly string _catalogId;
        private readonly string _seoLinksType;
        public List<CatalogProduct> Products { get; private set; }
        public List<Category> Categories { get; private set; }
        public List<SeoInfo> SeoInfos { get; private set; }
        public List<Catalog> Catalogs { get; private set; }

        public CatalogHierarchyHelper(string catalogId, string seoLinksType = StoreModule.Core.ModuleConstants.Settings.SEO.SeoLong)
        {
            _catalogId = catalogId;
            _seoLinksType = seoLinksType;

            Products = [];
            Categories = [];
            SeoInfos = [];
            Catalogs = [];
        }

        public void AddProduct(string productId, params string[] parentPaths)
        {
            var product = new CatalogProduct
            {
                Id = productId,
                Outlines = CreateOutlines(SeoProduct, productId, parentPaths, SeoInfos),
                IsActive = true
            };

            Products.Add(product);
        }

        public void AddCategory(string categoryId, params string[] parentPaths)
        {
            var category = new Category
            {
                Id = categoryId,
                Outlines = CreateOutlines(SeoCategory, categoryId, parentPaths, SeoInfos),
                IsActive = true
            };

            Categories.Add(category);
        }

        public void AddCatalog(string catalogId)
        {
            var catalog = new Catalog
            {
                Id = catalogId,

            };

            Catalogs.Add(catalog);
        }

        private static List<Outline> CreateOutlines(string objectType, string objectId, string[] parentPaths, IEnumerable<SeoInfo> seoInfos)
        {
            return parentPaths
                .Select(parentPath =>
                {
                    var outline = new Outline
                    {
                        Items = parentPath
                            .Split('/')
                            .Append(objectId)
                            .Select(id => new OutlineItem
                            {
                                Id = id,
                                SeoObjectType = SeoCategory,
                                SeoInfos = seoInfos
                                    .Where(s => s.ObjectId == id)
                                    .ToList()
                            })
                            .ToList(),
                    };
                    outline.Items.First().SeoObjectType = SeoCatalog;
                    outline.Items.Last().SeoObjectType = objectType;
                    return outline;
                })
                .ToList();
        }

        public void AddSeoInfo(string objectId, string objectType, string semanticUrl, bool isActive = true, string storeId = null, string languageCode = null)
        {
            var seoInfo = new SeoInfo
            {
                ObjectId = objectId,
                ObjectType = objectType,
                SemanticUrl = semanticUrl,
                IsActive = isActive,
                StoreId = storeId,
                LanguageCode = languageCode
            };

            SeoInfos.Add(seoInfo);
        }

        public CatalogSeoResolver CreateCatalogSeoResolver()
        {
            var catalogRepositoryMock = CreateCatalogRepositoryMock();
            var categoryServiceMock = CreateCategoryServiceMock();
            var productServiceMock = CreateProductServiceMock();
            var storeServiceMock = CreateStoreServiceMock();

            return new CatalogSeoResolver(
               catalogRepositoryMock.Object,
               categoryServiceMock.Object,
               productServiceMock.Object,
               storeServiceMock.Object);
        }

        public Mock<ICategoryService> CreateCategoryServiceMock()
        {
            var categoryServiceMock = new Mock<ICategoryService>();

            categoryServiceMock.Setup(x =>
               x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, bool clone) =>
                   { return Categories.Where(x => ids.Contains(x.Id)).ToList(); });

            categoryServiceMock.Setup(x =>
               x.GetByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, string catalogId) =>
                   { return Categories.Where(x => ids.Contains(x.Id)).ToList(); });

            return categoryServiceMock;
        }

        public Mock<IItemService> CreateProductServiceMock()
        {
            var productServiceMock = new Mock<IItemService>();

            productServiceMock.Setup(x =>
               x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, bool clone) =>
                   { return Products.Where(x => ids.Contains(x.Id)).ToList(); });

            productServiceMock.Setup(x =>
               x.GetByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, string categoryId) =>
                   { return Products.Where(x => ids.Contains(x.Id)).ToList(); });

            return productServiceMock;
        }

        public Mock<Func<ICatalogRepository>> CreateCatalogRepositoryMock()
        {
            var repositoryFactoryMock = new Mock<Func<ICatalogRepository>>();

            var seoInfoEntities = SeoInfos.Select(x => new SeoInfoEntity
            {
                ItemId = x.ObjectType == "CatalogProduct" ? x.ObjectId : null,
                CategoryId = x.ObjectType == "Category" ? x.ObjectId : null,
                CatalogId = x.ObjectType == "Catalog" ? x.ObjectId : null,
                Keyword = x.SemanticUrl,
                StoreId = x.StoreId,
                Language = x.LanguageCode,
                IsActive = x.IsActive
            }).ToList().AsQueryable();

            var productEntities = Products.Select(x => new ItemEntity
            {
                Id = x.Id,
                CatalogId = _catalogId,
                Code = x.Id,
                Name = x.Id,
                IsActive = x.IsActive.Value
            }).ToList().AsQueryable();

            var categoryEntities = Categories.Select(x => new CategoryEntity
            {
                Id = x.Id,
                CatalogId = _catalogId,
                Code = x.Id,
                Name = x.Id,
                IsActive = x.IsActive.Value
            }).ToList().AsQueryable();

            var catalogEntities = Catalogs.Select(x => new CatalogEntity
            {
                Id = x.Id,
                Name = x.Id,
                DefaultLanguage = "en-US",
            }).ToList().AsQueryable();

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb{Guid.NewGuid():N}")
                .Options;

            var context = new CatalogDbContext(options);
            context.Set<SeoInfoEntity>().AddRange(seoInfoEntities);
            context.SaveChanges();

            context.Set<ItemEntity>().AddRange(productEntities);
            context.SaveChanges();

            context.Set<CategoryEntity>().AddRange(categoryEntities);
            context.SaveChanges();

            context.Set<CatalogEntity>().AddRange(catalogEntities);
            context.SaveChanges();

            var repository = new Mock<ICatalogRepository>();
            repository.Setup(r => r.SeoInfos).Returns(context.Set<SeoInfoEntity>());
            repository.Setup(r => r.Items).Returns(context.Set<ItemEntity>());
            repository.Setup(r => r.Categories).Returns(context.Set<CategoryEntity>());
            repository.Setup(r => r.Catalogs).Returns(context.Set<CatalogEntity>());

            repositoryFactoryMock.Setup(f => f()).Returns(repository.Object);
            return repositoryFactoryMock;
        }

        public Mock<IStoreService> CreateStoreServiceMock()
        {
            var storeService = new Mock<IStoreService>();

            storeService.Setup(x =>
               x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, bool clone) =>
                   {
                       return [
                           new Store
                           {
                               Id = "B2B-store",
                               Catalog = _catalogId,
                               DefaultLanguage = "en-US",
                               Settings =
                               [
                                   new ObjectSettingEntry(StoreModule.Core.ModuleConstants.Settings.SEO.SeoLinksType)
                                   {
                                       Value = _seoLinksType,
                                   }
                               ]
                           }
                       ];
                   });

            return storeService;
        }
    }
}
