using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GenFu;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    public static class DeepCloneCheckerExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static async Task AssertClone(this ICloneable original)
        {
            await Task.Run(() =>
            {
                var clone = original.Clone();
                var sOriginal = JsonSerializer.Serialize((object)original, new JsonSerializerOptions() { WriteIndented = true });
                var sClone = JsonSerializer.Serialize(clone, new JsonSerializerOptions() { WriteIndented = true });
                Assert.Equal(sOriginal, sClone); // Ensure data in objects is equal
                original.IsDeepCloneOf(clone); // Ensure no shared references between objects (each object is a fully independent)
            });
        }

        public static void IsDeepCloneOf(this object original, object expected)
        {
            IsDeepCloneOf(original, expected, new List<object>(), original.GetType().Name);
        }

        private static void IsDeepCloneOf(object original, object expected, List<object> visited, string memberPath)
        {
            if (original != null && expected != null)
            {
                var typeOfOriginal = original.GetType();
                if (!IsPrimitive(typeOfOriginal) && !visited.Contains(original))
                {
                    visited.Add(original);
                    if (ReferenceEquals(original, expected))
                    {
                        throw new MemberAccessException(@$"Deep clone check failed: objects at path {memberPath} are reference equal.");
                    }
                    if (original is IEnumerable)
                    {
                        var originalEnumerator = ((IEnumerable)original).GetEnumerator();
                        var expectedEnumerator = ((IEnumerable)expected).GetEnumerator();
                        var iIdx = 0;
                        while (originalEnumerator.MoveNext())
                        {
                            expectedEnumerator.MoveNext();
                            IsDeepCloneOf(originalEnumerator.Current, expectedEnumerator.Current, visited, $@"{memberPath}[{iIdx++}]");
                        }
                    }
                    else
                    {
                        foreach (var propInfo in typeOfOriginal.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            IsDeepCloneOf(propInfo.GetValue(original), propInfo.GetValue(expected), visited, $@"{memberPath}.{propInfo.Name}");
                        }
                    }
                }
            }
            else if ((original == null && expected != null) || (original != null && expected == null))
            {
                throw new MemberAccessException(@$"Deep clone check failed: one of objects at path {memberPath} is null.");
            }
        }
    }

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

            await catalogProduct.AssertClone();
        }

        [Fact]
        public async Task CloneCatalog()
        {
            A.Configure<Catalog>()
                .Fill(x => x.Languages, A.ListOf<CatalogLanguage>(3))
                .Fill(x => x.Properties, A.ListOf<Property>(10));
            var catalog = A.New<Catalog>();

            await catalog.AssertClone();
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

            await category.AssertClone();
        }
    }
}
