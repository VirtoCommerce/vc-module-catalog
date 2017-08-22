using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class AssociationEntity : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the type of the association. 
        /// </summary>
        /// <value>
        /// The type of the association.
        /// </value>
        [StringLength(128)]
        [Required]
        public string AssociationType { get; set; }

        public int Priority { get; set; }

        public int? Quantity { get; set; }

        [StringLength(1024)]
        public string Tags { get; set; }

        #region Navigation Properties

        public string ItemId { get; set; }
        public virtual ItemEntity Item { get; set; }

        public string AssociatedItemId { get; set; }
        public virtual ItemEntity AssociatedItem { get; set; }

        public string AssociatedCategoryId { get; set; }
        public CategoryEntity AssociatedCategory { get; set; }
        #endregion

        public virtual ProductAssociation ToReferencedAssociationModel(ProductAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            association.Type = this.AssociationType;
            association.Priority = this.Priority;
            association.AssociatedObjectId = this.ItemId;
            association.Quantity = this.Quantity;

            if (this.Item != null)
            {
                association.AssociatedObject = this.Item.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, false);
                association.AssociatedObjectType = "product";
            }

            if (!this.Tags.IsNullOrEmpty())
            {
                association.Tags = this.Tags.Split(';');
            }

            return association;
        }

        public virtual ProductAssociation ToModel(ProductAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            association.Type = this.AssociationType;
            association.Priority = this.Priority;
            association.AssociatedObjectId = this.AssociatedItemId ?? this.AssociatedCategoryId;
            association.Quantity = this.Quantity;

            if (this.AssociatedCategory != null)
            {
                association.AssociatedObject = this.AssociatedCategory.ToModel(AbstractTypeFactory<Category>.TryCreateInstance());
                association.AssociatedObjectType = "category";
            }

            if (this.AssociatedItem != null)
            {
                association.AssociatedObject = this.AssociatedItem.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, false);
                association.AssociatedObjectType = "product";
            }

            if (!this.Tags.IsNullOrEmpty())
            {
                association.Tags = this.Tags.Split(';');
            }

            return association;
        }

        public virtual AssociationEntity FromModel(ProductAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            this.Priority = association.Priority;
            this.AssociationType = association.Type;
            this.Quantity = association.Quantity;

            if (association.AssociatedObjectType.EqualsInvariant("product"))
            {
                this.AssociatedItemId = association.AssociatedObjectId;
            }
            else if (association.AssociatedObjectType.EqualsInvariant("category"))
            {
                this.AssociatedCategoryId = association.AssociatedObjectId;
            }
            if (!association.Tags.IsNullOrEmpty())
            {
                this.Tags = string.Join(";", association.Tags);
            }

            return this;
        }

        public virtual void Patch(AssociationEntity target)
        {
            target.Priority = this.Priority;
            target.Tags = this.Tags;
            target.AssociationType = this.AssociationType;
            target.Quantity = this.Quantity;
        }
    }
}
