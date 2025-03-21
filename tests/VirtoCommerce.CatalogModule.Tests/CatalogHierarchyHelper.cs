using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.Extensions.SeoExtensions;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogHierarchyHelper
    {
        private readonly string _catalogId;
        public List<CatalogProduct> Products { get; private set; }
        public List<Category> Categories { get; private set; }
        public List<SeoInfo> SeoInfos { get; private set; }

        public CatalogHierarchyHelper(string catalogId)
        {
            _catalogId = catalogId;

            Products = [];
            Categories = [];
            SeoInfos = [];
        }

        public void AddProduct(string productId, params string[] parentPaths)
        {
            var product = new CatalogProduct
            {
                Id = productId,
                Outlines = CreateOutlines(SeoProduct, productId, parentPaths),
            };

            Products.Add(product);
        }

        public void AddCategory(string categoryId, params string[] parentPaths)
        {
            var category = new Category
            {
                Id = categoryId,
                Outlines = CreateOutlines(SeoCategory, categoryId, parentPaths),
            };

            Categories.Add(category);
        }

        private static List<Outline> CreateOutlines(string objectType, string objectId, string[] parentPaths)
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

            return categoryServiceMock;
        }

        public Mock<IItemService> CreateProductServiceMock()
        {
            var productServiceMock = new Mock<IItemService>();

            productServiceMock.Setup(x =>
               x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync((IList<string> ids, string responseGroup, bool clone) =>
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
                Keyword = x.SemanticUrl,
                StoreId = x.StoreId,
                Language = x.LanguageCode,
                IsActive = x.IsActive
            }).ToList().AsQueryable();

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb{Guid.NewGuid():N}")
                .Options;

            var context = new CatalogDbContext(options);
            context.Set<SeoInfoEntity>().AddRange(seoInfoEntities);
            context.SaveChanges();

            var repository = new Mock<ICatalogRepository>();
            repository.Setup(r => r.SeoInfos).Returns(context.Set<SeoInfoEntity>());

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
                       return [new Store { Catalog = _catalogId }];
                   });

            return storeService;
        }
    }
}
