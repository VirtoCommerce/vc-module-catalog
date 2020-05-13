using System.Text.Json;
using System.Threading.Tasks;
using GenFu;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "Unit")]
    public class EntityCloningTests
    {
        [Fact]
        public async Task CloneCatalogProduct()
        {
            A.Configure<Property>()
                .Fill(x => x.Attributes, A.ListOf<PropertyAttribute>(5))
                .Fill(x => x.DisplayNames, A.ListOf<PropertyDisplayName>(5))
                .Fill(x => x.ValidationRules, A.ListOf<PropertyValidationRule>(5))
                .Fill(x => x.Values, A.ListOf<PropertyValue>(5));

            A.Configure<OutlineItem>()
                .Fill(x => x.SeoInfos, A.ListOf<SeoInfo>(5));

            A.Configure<Outline>()
                .Fill(x => x.Items, A.ListOf<OutlineItem>(5));

            A.Configure<Catalog>()
                .Fill(x => x.Languages, A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, A.ListOf<Property>(10));

            A.Configure<Category>()
                .Fill(x => x.Images, A.ListOf<Image>(10))
                .Fill(x => x.Links, A.ListOf<CategoryLink>(10))
                .Fill(x => x.Outlines, A.ListOf<Outline>(5))
                .Fill(x => x.Properties, A.ListOf<Property>(10))
                .Fill(x => x.SeoInfos, A.ListOf<SeoInfo>(10));


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

            await Task.Run(() =>
            {
                var sCatalogProduct1st = JsonSerializer.Serialize(catalogProduct, new JsonSerializerOptions() { WriteIndented = true });
                var sCatalogProduct2nd = JsonSerializer.Serialize(catalogProduct.Clone(), new JsonSerializerOptions() { WriteIndented = true });
                Assert.Equal(sCatalogProduct1st, sCatalogProduct2nd);
            });
        }

        [Fact]
        public async Task CloneCatalog()
        {
            A.Configure<Catalog>()
                .Fill(x => x.Languages, A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, A.ListOf<Property>(10));
            var catalog = A.New<Catalog>();

            await Task.Run(() =>
            {
                var sCatalog1st = JsonSerializer.Serialize(catalog, new JsonSerializerOptions() { WriteIndented = true });
                var sCatalog2nd = JsonSerializer.Serialize(catalog.Clone(), new JsonSerializerOptions() { WriteIndented = true });
                Assert.Equal(sCatalog1st, sCatalog2nd);
            });
        }

        [Fact]
        public async Task CloneCategory()
        {
            A.Configure<Category>()
                .Fill(x => x.Images, A.ListOf<Image>(10))
                .Fill(x => x.Links, A.ListOf<CategoryLink>(10))
                .Fill(x => x.Outlines, A.ListOf<Outline>(5))
                .Fill(x => x.Properties, A.ListOf<Property>(10))
                .Fill(x => x.SeoInfos, A.ListOf<SeoInfo>(10));

            A.Configure<Catalog>()
                .Fill(x => x.Languages, A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, A.ListOf<Property>(10));

            A.Configure<OutlineItem>()
                .Fill(x => x.SeoInfos, A.ListOf<SeoInfo>(5));

            A.Configure<Outline>()
                .Fill(x => x.Items, A.ListOf<OutlineItem>(5));

            A.Configure<Property>()
                .Fill(x => x.Attributes, A.ListOf<PropertyAttribute>(5))
                .Fill(x => x.DisplayNames, A.ListOf<PropertyDisplayName>(5))
                .Fill(x => x.ValidationRules, A.ListOf<PropertyValidationRule>(5))
                .Fill(x => x.Values, A.ListOf<PropertyValue>(5));

            var category = A.New<Category>();
            category.Parents = A.ListOf<Category>(1).ToArray();

            await Task.Run(() =>
            {
                var sCategory1st = JsonSerializer.Serialize(category, new JsonSerializerOptions() { WriteIndented = true });
                var sCategory2st = JsonSerializer.Serialize(category.Clone(), new JsonSerializerOptions() { WriteIndented = true });
                Assert.Equal(sCategory1st, sCategory2st);
            });
        }
    }
}
