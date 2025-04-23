using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Model.ListEntry
{
    /// <summary>
    /// Base class for all entries used in catalog categories browsing.
    /// </summary>
	public class ListEntryBase : AuditableEntity, ISeoSupport, IHasOutlines, IHasRelevanceScore
    {
        /// <summary>
        /// Gets or sets the type. E.g. "product", "category"
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entry is active.
        /// </summary>
		public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
		public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the entry code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
		public string Code { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the links.
        /// </summary>
        /// <value>
        /// The links.
        /// </value>
		public IList<CategoryLink> Links { get; set; }

        public IList<Outline> Outlines { get; set; }

        /// <summary>
        /// All entry parents ids
        /// </summary>
        public IList<string> Outline { get; set; }

        /// <summary>
        /// All entry parents names
        /// </summary>
        public IList<string> Path { get; set; }

        /// <summary>
        /// Gets or sets the catalog id.
        /// </summary>
        public string CatalogId { get; set; }

        #region ISeoSupport members
        public virtual string SeoObjectType { get; set; }

        public virtual IList<SeoInfo> SeoInfos { get; set; }
        #endregion

        public double? RelevanceScore { get; set; }

        public virtual ListEntryBase FromModel(AuditableEntity entity)
        {
            // Entity
            Id = entity.Id;

            // AuditableEntity
            CreatedDate = entity.CreatedDate;
            ModifiedDate = entity.ModifiedDate;
            CreatedBy = entity.CreatedBy;
            ModifiedBy = entity.ModifiedBy;

            if (entity is ISeoSupport seoSupport)
            {
                SeoObjectType = seoSupport.SeoObjectType;
                SeoInfos = seoSupport.SeoInfos;
            }

            if (entity is IHasOutlines hasOutlines)
            {
                Outlines = hasOutlines.Outlines;

                // Use only physical catalog outline which this entity belongs to
                var firstOutline = hasOutlines.Outlines?.FirstOrDefault();
                if (firstOutline != null)
                {
                    Outline = firstOutline.Items.Select(x => x.Id).ToList();
                    Path = firstOutline.Items.Select(x => x.Name).ToList();
                }
            }

            if (entity is IHasRelevanceScore hasRelevanceScore)
            {
                RelevanceScore = hasRelevanceScore.RelevanceScore;
            }

            return this;
        }
    }
}
