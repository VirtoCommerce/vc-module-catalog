using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class CatalogExportExtensions
    {
        public static void ResetRedundantReferences(this object entity)
        {
            var product = entity as CatalogProduct;
            var category = entity as Category;
            var catalog = entity as Catalog;
            var asscociation = entity as ProductAssociation;
            var property = entity as Property;
            var propertyValue = entity as PropertyValue;

            if (propertyValue != null)
            {
                propertyValue.Property = null;
            }

            if (asscociation != null)
            {
                asscociation.AssociatedObject = null;
            }

            if (catalog != null)
            {
                catalog.Properties = null;
                foreach (var lang in catalog.Languages)
                {
                    lang.Catalog = null;
                }
            }

            if (category != null)
            {
                category.Catalog = null;
                category.Properties = null;
                category.Children = null;
                category.Parents = null;
                category.Outlines = null;
                if (category.PropertyValues != null)
                {
                    foreach (var propvalue in category.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
            }

            if (property != null)
            {
                property.Catalog = null;
                property.Category = null;
            }

            if (product != null)
            {
                product.Catalog = null;
                product.Category = null;
                product.Properties = null;
                product.MainProduct = null;
                product.Outlines = null;
                product.ReferencedAssociations = null;
                if (product.PropertyValues != null)
                {
                    foreach (var propvalue in product.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
                if (product.Associations != null)
                {
                    foreach (var association in product.Associations)
                    {
                        ResetRedundantReferences(association);
                    }
                }
                if (product.Variations != null)
                {
                    foreach (var variation in product.Variations)
                    {
                        ResetRedundantReferences(variation);
                    }
                }
            }
        }

        public static void LoadImages(this IHasImages[] haveImagesObjects, IBlobStorageProvider blobStorageProvider)
        {
            var allImages = haveImagesObjects
                .SelectMany(x => x.GetFlatObjectsListWithInterface<IHasImages>())
                .SelectMany(x => x.Images)
                .ToArray();

            foreach (var image in allImages)
            {
                using (var stream = blobStorageProvider.OpenRead(image.Url))
                {
                    image.BinaryData = stream.ReadFully();
                }
            }
        }
    }
}
