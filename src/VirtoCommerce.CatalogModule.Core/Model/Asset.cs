using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Asset : AssetBase
    {
        public Asset() : base(nameof(Asset))
        {
        }
        public string MimeType { get; set; }
        public long Size { get; set; }
        public string ReadableSize
        {
            get
            {
                return Size.ToHumanReadableSize();
            }
        }

        public byte[] BinaryData { get; set; }

        public override IList<SeoInfo> SeoInfos
        {
            get =>
                new List<SeoInfo>
                {
                    new SeoInfo
                    {
                        Name = Name,
                        MetaDescription = Description,
                        LanguageCode = LanguageCode,
                    }
                };
        }


        public override void TryInheritFrom(IEntity parent)
        {
            if (parent is Asset parentAsset)
            {
                MimeType = parentAsset.MimeType;
                Size = parentAsset.Size;
            }
            base.TryInheritFrom(parent);
        }
    }
}
