using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VirtoCommerce.CatalogModule.Client.Model
{
    /// <summary>
    /// CategoryLink
    /// </summary>
    [DataContract]
    public partial class CategoryLink :  IEquatable<CategoryLink>
    {
        /// <summary>
        /// Gets or Sets Priority
        /// </summary>
        [DataMember(Name="priority", EmitDefaultValue=false)]
        public int? Priority { get; set; }

        /// <summary>
        /// Gets or Sets SourceItemId
        /// </summary>
        [DataMember(Name="sourceItemId", EmitDefaultValue=false)]
        public string SourceItemId { get; set; }

        /// <summary>
        /// Gets or Sets SourceCategoryId
        /// </summary>
        [DataMember(Name="sourceCategoryId", EmitDefaultValue=false)]
        public string SourceCategoryId { get; set; }

        /// <summary>
        /// Gets or Sets CatalogId
        /// </summary>
        [DataMember(Name="catalogId", EmitDefaultValue=false)]
        public string CatalogId { get; set; }

        /// <summary>
        /// Gets or Sets CategoryId
        /// </summary>
        [DataMember(Name="categoryId", EmitDefaultValue=false)]
        public string CategoryId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CategoryLink {\n");
            sb.Append("  Priority: ").Append(Priority).Append("\n");
            sb.Append("  SourceItemId: ").Append(SourceItemId).Append("\n");
            sb.Append("  SourceCategoryId: ").Append(SourceCategoryId).Append("\n");
            sb.Append("  CatalogId: ").Append(CatalogId).Append("\n");
            sb.Append("  CategoryId: ").Append(CategoryId).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as CategoryLink);
        }

        /// <summary>
        /// Returns true if CategoryLink instances are equal
        /// </summary>
        /// <param name="other">Instance of CategoryLink to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CategoryLink other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.Priority == other.Priority ||
                    this.Priority != null &&
                    this.Priority.Equals(other.Priority)
                ) && 
                (
                    this.SourceItemId == other.SourceItemId ||
                    this.SourceItemId != null &&
                    this.SourceItemId.Equals(other.SourceItemId)
                ) && 
                (
                    this.SourceCategoryId == other.SourceCategoryId ||
                    this.SourceCategoryId != null &&
                    this.SourceCategoryId.Equals(other.SourceCategoryId)
                ) && 
                (
                    this.CatalogId == other.CatalogId ||
                    this.CatalogId != null &&
                    this.CatalogId.Equals(other.CatalogId)
                ) && 
                (
                    this.CategoryId == other.CategoryId ||
                    this.CategoryId != null &&
                    this.CategoryId.Equals(other.CategoryId)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)

                if (this.Priority != null)
                    hash = hash * 59 + this.Priority.GetHashCode();

                if (this.SourceItemId != null)
                    hash = hash * 59 + this.SourceItemId.GetHashCode();

                if (this.SourceCategoryId != null)
                    hash = hash * 59 + this.SourceCategoryId.GetHashCode();

                if (this.CatalogId != null)
                    hash = hash * 59 + this.CatalogId.GetHashCode();

                if (this.CategoryId != null)
                    hash = hash * 59 + this.CategoryId.GetHashCode();

                return hash;
            }
        }
    }
}
