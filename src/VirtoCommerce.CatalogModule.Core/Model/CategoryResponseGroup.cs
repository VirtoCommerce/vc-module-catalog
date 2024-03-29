using System;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    [Flags]
    public enum CategoryResponseGroup
    {
        None = 0,
        Info = 1,
        WithImages = 1 << 1,
        WithProperties = 1 << 2,
        WithLinks = 1 << 3,
        WithSeo = 1 << 4,
        WithParents = 1 << 5,
        WithOutlines = 1 << 6,
        WithDescriptions = 1 << 7,
        Full = Info | WithImages | WithProperties | WithLinks | WithSeo | WithParents | WithOutlines | WithDescriptions
    }
}
