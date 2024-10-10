using System;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    [Flags]
    public enum CatalogResponseGroup
    {
        None = 0,
        Info = 1,
        WithProperties = 1 << 2,
        WithSeo = 1 << 3,
        Full = Info | WithProperties | WithSeo
    }
}
