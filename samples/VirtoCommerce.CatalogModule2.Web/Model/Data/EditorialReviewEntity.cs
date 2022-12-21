using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class EditorialReviewEntity2 : EditorialReviewEntity
    {
        public override EditorialReview ToModel(EditorialReview review)
        {
            return base.ToModel(review);
        }
        public override EditorialReviewEntity FromModel(EditorialReview review, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(review, pkMap);
        }
        public override void Patch(EditorialReviewEntity target)
        {
            base.Patch(target);
        }
    }
}
