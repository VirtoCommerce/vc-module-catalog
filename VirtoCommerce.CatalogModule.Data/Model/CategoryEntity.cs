using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CategoryEntity : AuditableEntity
    {
        public CategoryEntity()
        {
            Images = new NullCollection<ImageEntity>();
            CategoryPropertyValues = new NullCollection<PropertyValueEntity>();
            OutgoingLinks = new NullCollection<CategoryRelationEntity>();
            IncommingLinks = new NullCollection<CategoryRelationEntity>();
            Properties = new NullCollection<PropertyEntity>();
        }

        [Required]
        [StringLength(64)]
        public string Code { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public int Priority { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [NotMapped]
        public CategoryEntity[] AllParents
        {
            get
            {
                var retVal = new CategoryEntity[] { };
                if(ParentCategory != null)
                {
                    retVal = ParentCategory.AllParents.Concat(new[] { ParentCategory }).ToArray();
                }
                return retVal;
            }
        }

        #region Navigation Properties
        [StringLength(128)]
        [ForeignKey("Catalog")]
        [Required]
        public string CatalogId { get; set; }

        public virtual CatalogEntity Catalog { get; set; }

        [StringLength(128)]
        [ForeignKey("ParentCategory")]
        public string ParentCategoryId { get; set; }

        public virtual CategoryEntity ParentCategory { get; set; }

        public virtual ObservableCollection<ImageEntity> Images { get; set; }

        public virtual ObservableCollection<PropertyValueEntity> CategoryPropertyValues { get; set; }
        //It new navigation property for link replace to stupid CategoryLink (will be removed later)
        public virtual ObservableCollection<CategoryRelationEntity> OutgoingLinks { get; set; }
        public virtual ObservableCollection<CategoryRelationEntity> IncommingLinks { get; set; }
        public virtual ObservableCollection<PropertyEntity> Properties { get; set; }
        #endregion


        public virtual Category ToModel(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.Id = this.Id;
            category.CreatedBy = this.CreatedBy;
            category.CreatedDate = this.CreatedDate;
            category.ModifiedBy = this.ModifiedBy;
            category.ModifiedDate = this.ModifiedDate;

            category.Code = this.Code;
            category.Name = this.Name;
            category.Priority = this.Priority;
            category.TaxType = this.TaxType;

            category.CatalogId = this.CatalogId;
         
            category.ParentId = this.ParentCategoryId;
            category.IsActive = this.IsActive;

            category.Links = this.OutgoingLinks.Select(x => x.ToModel(new CategoryLink())).ToList();
            category.Images = this.Images.OrderBy(x => x.SortOrder).Select(x => x.ToModel(AbstractTypeFactory<Image>.TryCreateInstance())).ToList();
            category.PropertyValues = this.CategoryPropertyValues.Select(x => x.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance())).ToList();
            category.Properties = this.Properties.Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToList();
          
            return category;
        }

        public virtual CategoryEntity FromModel(Category category, PrimaryKeyResolvingMap pkMap)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            pkMap.AddPair(category, this);

            this.Id = category.Id;
            this.CreatedBy = category.CreatedBy;
            this.CreatedDate = category.CreatedDate;
            this.ModifiedBy = category.ModifiedBy;
            this.ModifiedDate = category.ModifiedDate;

            this.Code = category.Code;
            this.Name = category.Name;
            this.Priority = category.Priority;
            this.TaxType = category.TaxType;
            this.CatalogId = category.CatalogId;

            this.ParentCategoryId = category.ParentId;
            this.EndDate = DateTime.UtcNow.AddYears(100);
            this.StartDate = DateTime.UtcNow;
            this.IsActive = category.IsActive ?? true;

            if (category.PropertyValues != null)
            {
                this.CategoryPropertyValues = new ObservableCollection<PropertyValueEntity>();
                foreach (var propertyValue in category.PropertyValues)
                {
                    if (!propertyValue.IsInherited && propertyValue.Value != null && !string.IsNullOrEmpty(propertyValue.Value.ToString()))
                    {
                        var dbPropertyValue = AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModel(propertyValue, pkMap);
                        this.CategoryPropertyValues.Add(dbPropertyValue);
                    }
                }
            }

            if (category.Links != null)
            {
                this.OutgoingLinks = new ObservableCollection<CategoryRelationEntity>(category.Links.Select(x => AbstractTypeFactory<CategoryRelationEntity>.TryCreateInstance().FromModel(x)));
            }

            if (category.Images != null)
            {
                this.Images = new ObservableCollection<ImageEntity>(category.Images.Select(x => AbstractTypeFactory<ImageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(CategoryEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.CatalogId = this.CatalogId;
            target.ParentCategoryId = this.ParentCategoryId;
            target.Code = this.Code;
            target.Name = this.Name;
            target.TaxType = this.TaxType;
            target.Priority = this.Priority;
            target.IsActive = this.IsActive;

            if (!this.CategoryPropertyValues.IsNullCollection())
            {
                this.CategoryPropertyValues.Patch(target.CategoryPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }

            if (!this.OutgoingLinks.IsNullCollection())
            {
                this.OutgoingLinks.Patch(target.OutgoingLinks, new LinkedCategoryComparer(), (sourceLink, targetLink) => sourceLink.Patch(targetLink));
            }

            if (!this.Images.IsNullCollection())
            {
                this.Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }

        }

    }
}
