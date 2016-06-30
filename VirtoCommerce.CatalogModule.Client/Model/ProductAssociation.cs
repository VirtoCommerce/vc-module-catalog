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
    /// ProductAssociation
    /// </summary>
    [DataContract]
    public partial class ProductAssociation :  IEquatable<ProductAssociation>
    {
        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets Priority
        /// </summary>
        [DataMember(Name="priority", EmitDefaultValue=false)]
        public int? Priority { get; set; }

        /// <summary>
        /// Gets or Sets AssociatedObjectId
        /// </summary>
        [DataMember(Name="associatedObjectId", EmitDefaultValue=false)]
        public string AssociatedObjectId { get; set; }

        /// <summary>
        /// Gets or Sets AssociatedObjectName
        /// </summary>
        [DataMember(Name="associatedObjectName", EmitDefaultValue=false)]
        public string AssociatedObjectName { get; set; }

        /// <summary>
        /// Gets or Sets AssociatedObjectType
        /// </summary>
        [DataMember(Name="associatedObjectType", EmitDefaultValue=false)]
        public string AssociatedObjectType { get; set; }

        /// <summary>
        /// Gets or Sets AssociatedObjectImg
        /// </summary>
        [DataMember(Name="associatedObjectImg", EmitDefaultValue=false)]
        public string AssociatedObjectImg { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ProductAssociation {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Priority: ").Append(Priority).Append("\n");
            sb.Append("  AssociatedObjectId: ").Append(AssociatedObjectId).Append("\n");
            sb.Append("  AssociatedObjectName: ").Append(AssociatedObjectName).Append("\n");
            sb.Append("  AssociatedObjectType: ").Append(AssociatedObjectType).Append("\n");
            sb.Append("  AssociatedObjectImg: ").Append(AssociatedObjectImg).Append("\n");
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
            return this.Equals(obj as ProductAssociation);
        }

        /// <summary>
        /// Returns true if ProductAssociation instances are equal
        /// </summary>
        /// <param name="other">Instance of ProductAssociation to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ProductAssociation other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.Type == other.Type ||
                    this.Type != null &&
                    this.Type.Equals(other.Type)
                ) && 
                (
                    this.Priority == other.Priority ||
                    this.Priority != null &&
                    this.Priority.Equals(other.Priority)
                ) && 
                (
                    this.AssociatedObjectId == other.AssociatedObjectId ||
                    this.AssociatedObjectId != null &&
                    this.AssociatedObjectId.Equals(other.AssociatedObjectId)
                ) && 
                (
                    this.AssociatedObjectName == other.AssociatedObjectName ||
                    this.AssociatedObjectName != null &&
                    this.AssociatedObjectName.Equals(other.AssociatedObjectName)
                ) && 
                (
                    this.AssociatedObjectType == other.AssociatedObjectType ||
                    this.AssociatedObjectType != null &&
                    this.AssociatedObjectType.Equals(other.AssociatedObjectType)
                ) && 
                (
                    this.AssociatedObjectImg == other.AssociatedObjectImg ||
                    this.AssociatedObjectImg != null &&
                    this.AssociatedObjectImg.Equals(other.AssociatedObjectImg)
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

                if (this.Type != null)
                    hash = hash * 59 + this.Type.GetHashCode();

                if (this.Priority != null)
                    hash = hash * 59 + this.Priority.GetHashCode();

                if (this.AssociatedObjectId != null)
                    hash = hash * 59 + this.AssociatedObjectId.GetHashCode();

                if (this.AssociatedObjectName != null)
                    hash = hash * 59 + this.AssociatedObjectName.GetHashCode();

                if (this.AssociatedObjectType != null)
                    hash = hash * 59 + this.AssociatedObjectType.GetHashCode();

                if (this.AssociatedObjectImg != null)
                    hash = hash * 59 + this.AssociatedObjectImg.GetHashCode();

                return hash;
            }
        }
    }
}
