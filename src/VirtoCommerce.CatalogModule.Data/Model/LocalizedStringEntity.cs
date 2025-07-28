using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model;

[Obsolete("Use VirtoCommerce.Platform.Data.Model.LocalizedStringEntity<T> instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
public abstract class LocalizedStringEntity<T> : Entity
    where T : Entity
{
    [Required]
    [StringLength(16)]
    public string LanguageCode { get; set; } = string.Empty; // e.g., "en-US"

    [Required]
    public string Value { get; set; } = string.Empty;

    public string ParentEntityId { get; set; } // Foreign key to the parent entity
    public virtual T ParentEntity { get; set; } = null!;
}
