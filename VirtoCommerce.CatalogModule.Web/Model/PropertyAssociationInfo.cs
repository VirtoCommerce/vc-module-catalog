using System;
using Omu.ValueInjecter;
using CatalogDomainModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Model
{
    /// <summary>
    /// Stores and applies Property associations
    /// </summary>
    public class PropertyAssociationInfo
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }

        public PropertyAssociationInfo FromEntity(CatalogDomainModel.Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            this.InjectFrom(property);

            return this;
        }

        public void Patch(CatalogDomainModel.Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            property.InjectFrom(this);
        }
    }
}
