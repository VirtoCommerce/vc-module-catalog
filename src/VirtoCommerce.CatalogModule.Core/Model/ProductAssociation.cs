using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class ProductAssociation : ValueObject, IHasOuterId, IHasImages
    {
        /// <summary>
        /// Association type (Accessories, Up-Sales, Cross-Sales, Related etc)
        /// </summary>
        public string Type { get; set; }

        public int Priority { get; set; }

        public int? Quantity { get; set; }
        /// <summary>
        /// Is a primary key of associating object
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// Each link element can have an associated object like Product, Category, etc.
        /// Is a primary key of associated object
        /// </summary>
        public string AssociatedObjectId { get; set; }
        /// <summary>
        /// Associated object type : 'product', 'category' etc
        /// </summary>
        public string AssociatedObjectType { get; set; }
        /// <summary>
        /// Associated object
        /// </summary>
        [JsonIgnore]
        public Entity AssociatedObject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OuterId { get; set; }

        /// <summary>
        /// Display name for associated object
        /// </summary>
        public virtual string AssociatedObjectName => (AssociatedObject is IHasName hasName) ? hasName.Name : null;
        /// <summary>
        /// Associated object image URL
        /// </summary>
        public virtual string AssociatedObjectImg => (AssociatedObject is IHasImages hasImages) ? hasImages.ImgSrc : null;

        public string[] Tags { get; set; }

        #region IHasImages members
        public string ImgSrc => AssociatedObjectImg;

        public IList<Image> Images
        {
            get
            {
                return (AssociatedObject is IHasImages hasImages) ? hasImages.Images : null;
            }
            set
            {
                if (AssociatedObject is IHasImages hasImages)
                {
                    hasImages.Images = value;
                }
            }
        }
        #endregion

        #region ICloneable
        public override object Clone()
        {
            var result = base.Clone() as ProductAssociation;

            result.Images = Images?.Select(x => x.Clone()).OfType<Image>().ToList();

            return result;
        }
        #endregion

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AssociatedObjectId;
            yield return AssociatedObjectType;
            yield return Type;
        }

    }
}
