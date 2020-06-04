using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class DynamicAssociationEntity : AuditableEntity, IHasOuterId
    {
        [Required]
        [StringLength(128)]
        public string AssociationType { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(128)]
        [Required]
        public string StoreId { get; set; }

        public int Priority { get; set; }

        [Required]
        public string ExpressionTreeSerialized { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        public virtual DynamicAssociation ToModel(DynamicAssociation dynamicAssociation)
        {
            if (dynamicAssociation == null)
            {
                throw new ArgumentNullException(nameof(dynamicAssociation));
            }

            dynamicAssociation.Id = Id;
            dynamicAssociation.CreatedBy = CreatedBy;
            dynamicAssociation.CreatedDate = CreatedDate;
            dynamicAssociation.ModifiedBy = ModifiedBy;
            dynamicAssociation.ModifiedDate = ModifiedDate;

            dynamicAssociation.OuterId = OuterId;
            dynamicAssociation.AssociationType = AssociationType;
            dynamicAssociation.IsActive = IsActive;
            dynamicAssociation.Description = Description;
            dynamicAssociation.StartDate = StartDate;
            dynamicAssociation.EndDate = EndDate;
            dynamicAssociation.Name = Name;
            dynamicAssociation.Priority = Priority;
            dynamicAssociation.StoreId = StoreId;
            dynamicAssociation.ExpressionTreeSerialized = ExpressionTreeSerialized;

            return dynamicAssociation;
        }

        public virtual DynamicAssociationEntity FromModel(DynamicAssociation dynamicAssociation, PrimaryKeyResolvingMap pkMap)
        {
            if (dynamicAssociation == null)
            {
                throw new ArgumentNullException(nameof(dynamicAssociation));
            }

            pkMap.AddPair(dynamicAssociation, this);

            Id = dynamicAssociation.Id;
            CreatedBy = dynamicAssociation.CreatedBy;
            CreatedDate = dynamicAssociation.CreatedDate;
            ModifiedBy = dynamicAssociation.ModifiedBy;
            ModifiedDate = dynamicAssociation.ModifiedDate;

            OuterId = dynamicAssociation.OuterId;
            AssociationType = dynamicAssociation.AssociationType;
            Name = dynamicAssociation.Name;
            Description = dynamicAssociation.Description;
            StartDate = dynamicAssociation.StartDate;
            EndDate = dynamicAssociation.EndDate;
            IsActive = dynamicAssociation.IsActive;
            StoreId = dynamicAssociation.StoreId;
            Priority = dynamicAssociation.Priority;
            ExpressionTreeSerialized = dynamicAssociation.ExpressionTreeSerialized;

            return this;
        }

        public virtual void Patch(DynamicAssociationEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Name = Name;
            target.IsActive = IsActive;
            target.Description = Description;
            target.StoreId = StoreId;
            target.StartDate = StartDate;
            target.EndDate = EndDate;
            target.Priority = Priority;
            target.AssociationType = AssociationType;
            target.ExpressionTreeSerialized = ExpressionTreeSerialized;
        }
    }
}
