using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class VideoEntity2 : VideoEntity
    {
        public override void Patch(VideoEntity target)
        {
            base.Patch(target);
        }
        public override VideoEntity FromModel(Video video, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(video, pkMap);
        }
        public override Video ToModel(Video video)
        {
            return base.ToModel(video);
        }
    }
}
