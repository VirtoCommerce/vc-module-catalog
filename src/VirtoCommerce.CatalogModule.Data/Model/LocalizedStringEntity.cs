using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Model;
public class LocalizedStringEntity<T> : Entity
    where T : Entity
{
    [StringLength(16)]
    [Required]
    public string LanguageCode { get; set; } = string.Empty; // e.g., "en-US"

    [Required]
    public string Value { get; set; } = string.Empty;

    public string ParentEntityId { get; set; } // Foreign key to the parent entity
    public virtual T ParentEntity { get; set; } = null!;

    public virtual LocalizedString ToModel(LocalizedString item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        return item;
    }

    public virtual LocalizedStringEntity<T> FromModel(string languageCode, string value)
    {
        ArgumentNullException.ThrowIfNull(languageCode, nameof(languageCode));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        LanguageCode = languageCode;
        Value = value;

        return this;
    }

    public virtual void Patch(LocalizedStringEntity<T> target)
    {
    }
}
