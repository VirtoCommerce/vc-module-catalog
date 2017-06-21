using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class PropertyAttributeEntity : AuditableEntity
    {
        [Required]
        [StringLength(128)]
        public string PropertyAttributeName { get; set; }

        [Required]
        [StringLength(128)]
        public string PropertyAttributeValue { get; set; }

        public int Priority { get; set; }

        #region Navigation Properties

        public string PropertyId { get; set; }

        public virtual PropertyEntity Property { get; set; }

        #endregion

        public virtual PropertyAttribute ToModel(PropertyAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            attribute.Id = this.Id;
            attribute.CreatedBy = this.CreatedBy;
            attribute.CreatedDate = this.CreatedDate;
            attribute.ModifiedBy = this.ModifiedBy;
            attribute.ModifiedDate = this.ModifiedDate;
            
            attribute.Name = this.PropertyAttributeName;
            attribute.Value = this.PropertyAttributeValue;
            attribute.PropertyId = this.PropertyId;

            return attribute;
        }

        public virtual PropertyAttributeEntity FromModel(PropertyAttribute attribute, PrimaryKeyResolvingMap pkMap)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            pkMap.AddPair(attribute, this);

            this.Id = attribute.Id;
            this.CreatedBy = attribute.CreatedBy;
            this.CreatedDate = attribute.CreatedDate;
            this.ModifiedBy = attribute.ModifiedBy;
            this.ModifiedDate = attribute.ModifiedDate;    
            this.PropertyId = attribute.PropertyId;
            this.PropertyAttributeName = attribute.Name;
            this.PropertyAttributeValue = attribute.Value;

            return this;
        }

        public virtual void Patch(PropertyAttributeEntity target)
        {
            target.PropertyAttributeName = this.PropertyAttributeName;
            target.PropertyAttributeValue = this.PropertyAttributeValue;           
        }
    }
}
