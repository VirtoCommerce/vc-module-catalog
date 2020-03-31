using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    /// <summary>
    /// This interface is used for types which support copy operation. Copy is very similar to clone operations,
    /// but it in additional this operation may reset the primary keys for copying objects 
    /// </summary>
    public interface ICopyable
    {
        object GetCopy();
    }
}
