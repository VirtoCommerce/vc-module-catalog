using System;

namespace VirtoCommerce.CatalogModule.Core.Model.Configuration;

[Flags]
public enum ProductConfigurationResponseGroup
{
    None = 0,
    /// <summary>
    /// Include the configuration's Sections collection.
    /// </summary>
    Sections = 1 << 0,
    /// <summary>
    /// Include each section's Options collection. <see cref="Sections"/> is implied —
    /// the repository normalizes the flags so callers passing <c>Options</c> alone still
    /// get Sections loaded (Options cannot exist out their parent Section).
    /// </summary>
    Options = 1 << 1,
    /// <summary>
    /// Load the Items (CatalogProducts) referenced by each option's ProductId into the same
    /// repository tracker, so the read-side ResolveImageUrls can populate option.Product.Images.
    /// Should NOT be requested by save paths — it tracks unrelated Item rows that bring the
    /// Item ↔ ProductConfiguration 1:1 cascade-delete relationship under EF fixup.
    /// </summary>
    Products = 1 << 2,
    /// <summary>
    /// Full graph for read APIs: sections, their options, and option-referenced products.
    /// </summary>
    Full = Sections | Options | Products,
}
