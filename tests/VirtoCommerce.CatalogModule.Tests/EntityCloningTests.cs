using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "Unit")]
    public class EntityCloningTests
    {
        /* // Intentionally temporary disabled due to memory overhead with cloning.
        [Fact]
        public async Task CloneCatalogProduct()
        {
            A.Configure<Property>()
                .Fill(x => x.Attributes, x => A.ListOf<PropertyAttribute>(5))
                .Fill(x => x.DisplayNames, x => A.ListOf<PropertyDisplayName>(5))
                .Fill(x => x.ValidationRules, x => A.ListOf<PropertyValidationRule>(5))
                .Fill(x => x.Values, x => A.ListOf<PropertyValue>(5));

            A.Configure<OutlineItem>()
                .Fill(x => x.SeoInfos, x => A.ListOf<SeoInfo>(5));

            A.Configure<Outline>()
                .Fill(x => x.Items, x => A.ListOf<OutlineItem>(5));

            A.Configure<Catalog>()
                .Fill(x => x.Languages, x => A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, x => A.ListOf<Property>(10));

            A.Configure<Category>()
                .Fill(x => x.Images, x => A.ListOf<Image>(10))
                .Fill(x => x.Links, x => A.ListOf<CategoryLink>(10))
                .Fill(x => x.Outlines, x => A.ListOf<Outline>(5))
                .Fill(x => x.Properties, x => A.ListOf<Property>(10))
                .Fill(x => x.SeoInfos, x => A.ListOf<SeoInfo>(10));

            var catalogProduct = A.New<CatalogProduct>();
            catalogProduct.Assets = A.ListOf<Asset>(10);
            catalogProduct.Associations = A.ListOf<ProductAssociation>(10);
            catalogProduct.Catalog = A.New<Catalog>();
            catalogProduct.Category = A.New<Category>();
            catalogProduct.Images = A.ListOf<Image>(10);
            catalogProduct.Links = A.ListOf<CategoryLink>(10);
            catalogProduct.Outlines = A.ListOf<VirtoCommerce.CoreModule.Core.Outlines.Outline>(10);
            catalogProduct.Properties = A.ListOf<Property>(10);
            catalogProduct.ReferencedAssociations = A.ListOf<ProductAssociation>(10);
            catalogProduct.Reviews = A.ListOf<EditorialReview>(10);
            catalogProduct.SeoInfos = A.ListOf<SeoInfo>(10);
            catalogProduct.Variations = A.ListOf<Variation>(10);

            await catalogProduct.AssertCloneIndependency();
        }

        [Fact]
        public async Task CloneCatalog()
        {
            A.Configure<Catalog>()
                .Fill(x => x.Languages, x => A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, x => A.ListOf<Property>(10));
            var catalog = A.New<Catalog>();

            await catalog.AssertCloneIndependency();
        }

        [Fact]
        public async Task CloneCategory()
        {
            A.Configure<Category>()
                .Fill(x => x.Images, x => A.ListOf<Image>(10))
                .Fill(x => x.Links, x => A.ListOf<CategoryLink>(10))
                .Fill(x => x.Outlines, x => A.ListOf<Outline>(5))
                .Fill(x => x.Properties, x => A.ListOf<Property>(10))
                .Fill(x => x.SeoInfos, x => A.ListOf<SeoInfo>(10));

            A.Configure<Catalog>()
                .Fill(x => x.Languages, x => A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, x => A.ListOf<Property>(10));

            A.Configure<OutlineItem>()
                .Fill(x => x.SeoInfos, x => A.ListOf<SeoInfo>(5));

            A.Configure<Outline>()
                .Fill(x => x.Items, x => A.ListOf<OutlineItem>(5));

            A.Configure<Property>()
                .Fill(x => x.Attributes, x => A.ListOf<PropertyAttribute>(5))
                .Fill(x => x.DisplayNames, x => A.ListOf<PropertyDisplayName>(5))
                .Fill(x => x.ValidationRules, x => A.ListOf<PropertyValidationRule>(5))
                .Fill(x => x.Values, x => A.ListOf<PropertyValue>(5));

            var category = A.New<Category>();
            category.Parents = A.ListOf<Category>(1).ToArray();

            await category.AssertCloneIndependency();
        }
        */
    }
}
