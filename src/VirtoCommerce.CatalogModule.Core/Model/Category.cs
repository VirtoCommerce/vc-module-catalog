using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Category : AuditableEntity, IHasLinks, ISeoSupport, IHasOutlines, IHasImages, IHasProperties, IHasTaxType, IHasName, IHasOuterId, IExportable, IHasExcludedProperties, IHasRelevanceScore
    {
        public Category()
        {
            IsActive = true;
        }
        public string CatalogId { get; set; }
        [JsonIgnore]
        public Catalog Catalog { get; set; }

        public string ParentId { get; set; }
        [JsonIgnore]
        public Category Parent { get; set; }
        public string Code { get; set; }

        public string Name { get; set; }

        public LocalizedString LocalizedName { get; set; }

        /// <summary>
        /// Category outline in physical catalog (all parent categories ids concatenated. E.g. (1/21/344))
        /// </summary>
        public string Outline => Parent != null ? $"{Parent.Outline}/{Id}" : Id;
        /// <summary>
        /// Category path in physical catalog (all parent categories names concatenated. E.g. (parent1/parent2))
        /// </summary>
        public string Path
        {
            get
            {
                return _path ?? (Parent != null ? $"{Parent.Path}/{Name}" : Name);
            }
            set
            {
                _path = value;
            }
        }

        private string _path;

        public bool IsVirtual { get; set; }
        public int Level { get; set; }
        [JsonIgnore]
        public Category[] Parents { get; set; }

        // Type of product package (set of package types with their specific dimensions) can be inherited by nested products and categories
        public string PackageType { get; set; }

        public int Priority { get; set; }

        public bool? IsActive { get; set; }
        public string OuterId { get; set; }
        [JsonIgnore]
        public IList<Category> Children { get; set; }

        #region IHasProperties members
        public IList<Property> Properties { get; set; }
        #endregion

        #region IHasExcludedProperties members
        public IList<ExcludedProperty> ExcludedProperties { get; set; }
        #endregion

        #region ILinkSupport members
        public IList<CategoryLink> Links { get; set; }
        #endregion

        #region IHasTaxType members
        public string TaxType { get; set; }
        #endregion
        public string SeoObjectType { get { return GetType().Name; } }
        public IList<SeoInfo> SeoInfos { get; set; }

        public bool? EnableDescription { get; set; }
        public IList<CategoryDescription> Descriptions { get; set; }

        #region IHasImages members
        /// <summary>
        /// Gets the default image
        /// </summary>
        /// <value>
        /// The image source URL.
        /// </value>
        public string ImgSrc
        {
            get
            {
                string result = null;
                if (Images != null && Images.Any())
                {
                    result = Images.MinBy(x => x.SortOrder)?.Url;
                }
                return result;
            }
        }
        public IList<Image> Images { get; set; }
        #endregion

        #region IHasOutlines members
        public IList<Outline> Outlines { get; set; }
        #endregion

        #region IInheritable members
        /// <summary>
        /// System flag used to mark that object was inherited from other
        /// </summary>
        public virtual bool IsInherited => false;

        public virtual void TryInheritFrom(IEntity parent)
        {
            this.InheritExcludedProperties(parent as IHasExcludedProperties);

            if (parent is IHasProperties hasProperties)
            {
                // Properties inheritance
                foreach (var parentProperty in hasProperties.Properties ?? Array.Empty<Property>())
                {
                    if (this.HasPropertyExcluded(parentProperty.Name))
                    {
                        continue;
                    }

                    Properties ??= new List<Property>();
                    // Try to find property by type and name
                    var existProperty = Properties.FirstOrDefault(x => x.IsSame(parentProperty));
                    if (existProperty == null)
                    {
                        existProperty = AbstractTypeFactory<Property>.TryCreateInstance();
                        Properties.Add(existProperty);
                    }
                    existProperty.TryInheritFrom(parentProperty);
                    existProperty.IsReadOnly = existProperty.Type != PropertyType.Category;
                }
                // Restore order after changes
                Properties = Properties?.OrderBy(x => x.Name).ToList();
            }

            if (parent is IHasTaxType hasTaxType)
            {
                // Try to inherit taxType from parent category
                TaxType ??= hasTaxType.TaxType;
            }
        }
        #endregion

        #region ICloneable
        public virtual object Clone()
        {
            var result = (Category)MemberwiseClone();

            result.SeoInfos = SeoInfos?.Select(x => x.CloneTyped()).ToList();
            result.Children = Children?.Select(x => x.CloneTyped()).ToList();
            result.Outlines = Outlines?.Select(x => x.CloneTyped()).ToList();
            result.Parents = Parents?.Select(x => x.CloneTyped()).ToArray();
            result.Properties = Properties?.Select(x => x.CloneTyped()).ToList();
            result.Links = Links?.Select(x => x.CloneTyped()).ToList();
            result.Descriptions = Descriptions?.Select(x => x.CloneTyped()).ToList();
            //result.Images = Images?.Select(x => x.CloneTyped()).ToList(); // Intentionally temporary disabled due to memory overhead
            result.LocalizedName = LocalizedName?.CloneTyped();

            return result;
        }
        #endregion

        public bool ParentIsActive
        {
            get
            {
                return Parent is null ||
                       Parent.IsActive == true && Parent.ParentIsActive;
            }
        }

        public double? RelevanceScore { get; set; }

        public virtual void Move(string catalogId, string categoryId)
        {
            CatalogId = catalogId;
            ParentId = categoryId;
        }

        /// <summary>
        /// Reduce Category details
        /// </summary>
        /// <param name="responseGroup">A set of necessary groups</param>
        public virtual void ReduceDetails(string responseGroup)
        {
            // Reduce details according to response group
            var categoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CategoryResponseGroup.Full);

            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithImages))
            {
                Images = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithLinks))
            {
                Links = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithParents))
            {
                Parents = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithProperties))
            {
                Properties = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithOutlines))
            {
                Outlines = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithSeo))
            {
                SeoInfos = null;
            }
            if (!categoryResponseGroup.HasFlag(CategoryResponseGroup.WithDescriptions))
            {
                Descriptions = null;
            }
        }
    }
}
