using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    public interface IOutlinePartNameResolver
    {
        string ResolveOutlineName(IEntity entity);

        LocalizedString ResolveLocalizedOutlineName(IEntity entity);
    }
}
