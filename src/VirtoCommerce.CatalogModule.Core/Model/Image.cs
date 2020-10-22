using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Seo;
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

        public override IList<SeoInfo> SeoInfos
        {
            get
            {
                var result = base.SeoInfos;
                foreach (var seoInfo in result)
                {
                    seoInfo.ImageAltDescription = AltText;
                }

                return result;

            }
        }

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
