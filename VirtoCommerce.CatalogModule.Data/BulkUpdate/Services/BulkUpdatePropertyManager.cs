using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdatePropertyManager : IBulkUpdatePropertyManager
    {
        private readonly IPagedDataSourceFactory _dataSourceFactory;

        public BulkUpdatePropertyManager(IPagedDataSourceFactory dataSourceFactory)
        {
            _dataSourceFactory = dataSourceFactory;
        }

        public virtual Property[] GetProperties(ProductBulkUpdateDataQuery dataQuery)
        {
            // Maybe could get all product inherited properties faster, by getting all outlines for categories and get all properties with categoryId or CatalogId
            var dataSource = _dataSourceFactory.Create(dataQuery);
            var result = new List<Property>();
            var propertyIds = new HashSet<string>();

            while (dataSource.Fetch())
            {
                var newProperties = dataSource.Items
                    .Cast<CatalogProduct>()
                    .SelectMany(x => x.Properties.Where(y => y.IsInherited))
                    .Distinct(AnonymousComparer.Create<Property, string>(x => x.Id))
                    .Where(x => !propertyIds.Contains(x.Id));


                propertyIds.AddRange(newProperties.Select(x => x.Id));
                result.AddRange(newProperties);
            }

            result.AddRange(GetStandardProperties());

            return result.ToArray();
        }

        protected virtual IEnumerable<Property> GetStandardProperties()
        {
            yield return new Property() { Name = nameof(CatalogProduct.Name), Type = PropertyType.Product, ValueType = PropertyValueType.LongText, };
            yield return new Property() { Name = nameof(CatalogProduct.StartDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, };
            yield return new Property() { Name = nameof(CatalogProduct.EndDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, };
            yield return new Property() { Name = nameof(CatalogProduct.Priority), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, };
            yield return new Property() { Name = nameof(CatalogProduct.EnableReview), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.IsActive), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.IsBuyable), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.TrackInventory), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.MinQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, };
            yield return new Property() { Name = nameof(CatalogProduct.MaxQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, };
            yield return new Property() { Name = nameof(CatalogProduct.Vendor), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, };
            yield return new Property() { Name = nameof(CatalogProduct.WeightUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, };
            yield return new Property() { Name = nameof(CatalogProduct.Weight), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.MeasureUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, };
            yield return new Property() { Name = nameof(CatalogProduct.PackageType), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, };
            yield return new Property() { Name = nameof(CatalogProduct.Height), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.Width), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.Length), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
        }

        public virtual void UpdateProperties(CatalogProduct[] products, PropertyValue[] propertyValues)
        {
            throw new NotImplementedException();
        }
    }
}
