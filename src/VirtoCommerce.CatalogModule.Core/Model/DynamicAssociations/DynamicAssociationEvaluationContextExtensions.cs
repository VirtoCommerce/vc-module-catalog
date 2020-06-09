using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations
{
    public static class DynamicAssociationEvaluationContextExtensions
    {
        #region DynamicAssociationEvaluationContext extensions

        public static bool IsItemInCategory(this DynamicAssociationEvaluationContext context, string[] categoryIds, string[] excludingCategoryIds, string[] excludingProductIds)
        {
            var result = context.Products.InCategories(categoryIds)
                               .ExcludeCategories(excludingCategoryIds)
                               .ExcludeProducts(excludingProductIds)
                               .Any();
            return result;
        }

        public static bool AreItemPropertyValuesEqual(this DynamicAssociationEvaluationContext context, Dictionary<string, string> propertyValues)
        {
            var result = context.Products.WithPropertyValues(propertyValues).Any();

            return result;
        }

        #endregion DynamicAssociationEvaluationContext extensions

        #region CatalogProduct extensions

        public static IEnumerable<CatalogProduct> ExcludeCategories(this IEnumerable<CatalogProduct> products, string[] categoryIds)
        {
            var retVal = products.Where(x => !ProductInCategories(x, categoryIds));

            return retVal;
        }

        public static IEnumerable<CatalogProduct> ExcludeProducts(this IEnumerable<CatalogProduct> products, string[] productIds)
        {
            var retval = products.Where(x => !ProductInProducts(x, productIds));

            return retval;
        }

        public static IEnumerable<CatalogProduct> InCategories(this IEnumerable<CatalogProduct> products, string[] categoryIds)
        {
            categoryIds = categoryIds.Where(x => x != null).ToArray();

            return categoryIds.Any() ? products.Where(x => ProductInCategories(x, categoryIds)) : products;
        }

        public static bool ProductInCategories(this CatalogProduct product, ICollection<string> categoryIds)
        {
            var result = categoryIds.Contains(product.CategoryId, StringComparer.OrdinalIgnoreCase);

            if (!result && !product.Outlines.IsNullOrEmpty())
            {
                result = product.Outlines.Any(x => x.Items.Select(x => x.Id).Intersect(categoryIds, StringComparer.OrdinalIgnoreCase).Any());
            }

            return result;
        }

        public static bool ProductInProducts(this CatalogProduct product, IEnumerable<string> productIds)
        {
            return productIds.Contains(product.Id, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<CatalogProduct> WithPropertyValues(this IEnumerable<CatalogProduct> products, Dictionary<string, string> propertyValues)
        {
            var productArray = products as CatalogProduct[] ?? products.ToArray();

            return propertyValues.Any() ? productArray.Where(x => x.ProductHasPropertyValues(propertyValues)) : productArray;
        }

        public static bool ProductHasPropertyValues(this CatalogProduct product, Dictionary<string, string> propertyValues)
        {
            var result = propertyValues.Where(x => x.Key != null).All(property =>
            {
                var result = false;
                var productProperty = product.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(property.Key));

                result = productProperty != null && productProperty.Values.Any(x => x.Value?.ToString().EqualsInvariant(property.Value) ?? false);

                return result;
            });

            return result;
        }

        #endregion CatalogProduct extensions
    }
}
