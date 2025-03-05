using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;
using StoreSettings = VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;

namespace VirtoCommerce.CatalogModule.Tests;

[Trait("Category", "CI")]
public class OutlineExtensionsTests
{
    private readonly Store _store = new()
    {
        Id = "Store1",
        DefaultLanguage = "en-US",
        Languages = ["en-US", "ru-RU"],
        Settings = [new ObjectSettingEntry { Name = StoreSettings.SeoLinksType.Name, Value = "Collapsed" }],
    };

    [Fact]
    public void When_HasMultipleOutlines_Expect_PathForGivenCatalog()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new() { SeoObjectType = "Catalog",  Id = "catalog1" },
                    new() { SeoObjectType = "Category", Id = "parent1" },
                    new() { SeoObjectType = "Category", Id = "category1",
                    },
                },
            },
            new()
            {
                Items = new List<OutlineItem>
                {
                    new() { SeoObjectType = "Catalog",  Id = "catalog2" },
                    new() { SeoObjectType = "Category", Id = "parent2" },
                    new() { SeoObjectType = "Category", Id = "category2" },
                },
            },
        };

        var result = outlines.GetOutlinePath("catalog2");
        Assert.Equal("parent2/category2", result);
    }

    [Fact]
    public void When_HasNoSeoRecords_Expect_Null()
    {
        var result = ((IEnumerable<Outline>)null).GetSeoPath(_store, "en-US");
        Assert.Null(result);
    }

    // Category tests

    [Fact]
    public void When_CategoryHasSeoRecords_Expect_ShortPath()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("category2", result);
    }

    [Fact]
    public void When_CategoryHasParentSeoRecords_Expect_LongPath()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "grandparent1" },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "grandparent2" },
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("grandparent2/parent2/category2", result);
    }

    [Fact]
    public void When_CategoryMissingAnyParentSeoRecord_Expect_Null()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>(),
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Null(result);
    }

    [Fact]
    public void When_CategoryHasInactiveSeoRecords_Expect_OnlyActiveRecords()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "inactive-parent1", IsActive = false },
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "active-parent1", IsActive = true },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "inactive-parent2", IsActive = false },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "active-parent2", IsActive = true },
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "inactive-category1", IsActive = false },
                            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "active-category1", IsActive = true },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "inactive-category2", IsActive = false },
                            new() { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "active-category2", IsActive = true },
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("active-parent2/active-category2", result);
    }

    [Fact]
    public void When_CategoryHasVirtualParent_Expect_SkipLinkedPhysicalParent()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "physical-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "physical-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("virtual-parent2/category2", result);
    }

    [Fact]
    public void When_CategoryIsLinkedToCatalogRoot_Expect_Null()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Null(result);
    }

    [Fact]
    public void When_CategoryParentIsLinkedToCatalogRoot_Expect_SkipLinkedPhysicalParent()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "physical-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "physical-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("category2", result);
    }

    [Fact]
    public void When_CategoryLastItemHasVirtualParent_Expect_Null()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Null(result);
    }

    // Product tests

    [Fact]
    public void When_ProductHasSeoRecordsAndNoCategorySeoRecords_Expect_Null()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Null(result);
    }

    [Fact]
    public void When_ProductHasCategorySeoRecords_Expect_LongPath()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("category2/product2", result);
    }

    [Fact]
    public void When_ProductHasParentCategorySeoRecords_Expect_LongPath()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "grandparent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "grandparent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("grandparent2/parent2/category2/product2", result);
    }

    [Fact]
    public void When_ProductMissingAnyParentSeoRecord_Expect_Null()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>(),
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Null(result);
    }

    [Fact]
    public void When_ProductHasParentCategoryWithVirtualParent_Expect_SkipLinkedPhysicalParent()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("virtual-parent2/product2", result);
    }

    [Fact]
    public void When_ProductHasVirtualParent_Expect_KeepProduct()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "Category",
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2"},
                        },
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("virtual-parent2/product2", result);
    }

    [Fact]
    public void When_ProductIsLinkedToCatalogRoot_Expect_KeepLinkedProduct()
    {
        var outlines = new List<Outline>
        {
            new()
            {
                Items = new List<OutlineItem>
                {
                    new()
                    {
                        SeoObjectType = "Catalog",
                    },
                    new()
                    {
                        SeoObjectType = "CatalogProduct",
                        HasVirtualParent = true,
                        SeoInfos = new List<SeoInfo>
                        {
                            new() {StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1"},
                            new() {StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2"},
                        },
                    },
                },
            },
        };

        var result = outlines.GetSeoPath(_store, "ru-RU");
        Assert.Equal("product2", result);
    }
}
