using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Image : AssetBase
    {
        public Image() : base(nameof(Image))
        {
        }
        public byte[] BinaryData { get; set; }

        public string AltText { get; set; }

        public override void TryInheritFrom(IEntity parent)
        {
            if (parent is Image parentImage)
            {
                SortOrder = parentImage.SortOrder;
            }
            base.TryInheritFrom(parent);
        }
    }
}
