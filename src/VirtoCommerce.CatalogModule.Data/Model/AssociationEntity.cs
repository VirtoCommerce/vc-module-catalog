using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class AssociationEntity : AuditableEntity, IHasOuterId
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

        [StringLength(128)]
        public string OuterId { get; set; }

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

            association.OuterId = OuterId;
            association.Type = AssociationType;
            association.Priority = Priority;
            association.AssociatedObjectId = ItemId;
            association.Quantity = Quantity;

            if (Item != null)
            {
                association.AssociatedObject = Item.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, false);
                association.AssociatedObjectType = "product";
            }

            if (!Tags.IsNullOrEmpty())
            {
                association.Tags = Tags.Split(';');
            }

            return association;
        }

        public virtual ProductAssociation ToModel(ProductAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            association.Id = Id;
            association.OuterId = OuterId;
            association.Type = AssociationType;
            association.Priority = Priority;
            association.AssociatedObjectId = AssociatedItemId ?? AssociatedCategoryId;
            association.Quantity = Quantity;
            association.ItemId = ItemId;
            if (AssociatedCategory != null)
            {
                association.AssociatedObject = AssociatedCategory.ToModel(AbstractTypeFactory<Category>.TryCreateInstance());
                association.AssociatedObjectType = "category";
            }

            if (AssociatedItem != null)
            {
                association.AssociatedObject = AssociatedItem.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, false);
                association.AssociatedObjectType = "product";
            }

            if (!Tags.IsNullOrEmpty())
            {
                association.Tags = Tags.Split(';');
            }

            return association;
        }

        public virtual AssociationEntity FromModel(ProductAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            Id = association.Id;
            Priority = association.Priority;
            AssociationType = association.Type;
            Quantity = association.Quantity;
            ItemId = association.ItemId;

            if (association.AssociatedObjectType.EqualsIgnoreCase("product"))
            {
                AssociatedItemId = association.AssociatedObjectId;
            }
            else if (association.AssociatedObjectType.EqualsIgnoreCase("category"))
            {
                AssociatedCategoryId = association.AssociatedObjectId;
            }

            if (!association.Tags.IsNullOrEmpty())
            {
                Tags = string.Join(";", association.Tags);
            }

            return this;
        }

        public virtual void Patch(AssociationEntity target)
        {
            target.Priority = Priority;
            target.Tags = Tags;
            target.AssociationType = AssociationType;
            target.Quantity = Quantity;
            target.ItemId = ItemId;
            target.AssociatedItemId = AssociatedItemId;
            target.AssociatedCategoryId = AssociatedCategoryId;
        }
    }

    public class AssociationEntityComparer : IEqualityComparer<AssociationEntity>
    {
        public bool Equals(AssociationEntity x, AssociationEntity y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }


            var result = x.Id == y.Id;

            if (x.IsTransient() || y.IsTransient())
            {
                result = x.AssociationType == y.AssociationType && x.AssociatedItemId == y.AssociatedItemId && x.ItemId == y.ItemId && x.AssociatedCategoryId == y.AssociatedCategoryId;
            }

            return result;
        }

        public int GetHashCode(AssociationEntity obj)
        {
            return obj != null ? HashCode.Combine(obj.AssociationType, obj.AssociatedItemId, obj.AssociatedCategoryId, obj.ItemId) : 0;
        }
    }
}
