using Omu.ValueInjecter;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class EditorialReviewConverter
    {
        public static webModel.EditorialReview ToWebModel(this moduleModel.EditorialReview review)
        {
            var retVal = new webModel.EditorialReview();

            retVal.Content = review.Content;
            retVal.Id = review.Id;
            retVal.IsInherited = review.IsInherited;
            retVal.LanguageCode = review.LanguageCode;
            retVal.ReviewType = review.ReviewType;
            
            
            return retVal;
        }

        public static moduleModel.EditorialReview ToCoreModel(this webModel.EditorialReview review)
        {
            var retVal = new moduleModel.EditorialReview();
            retVal.InjectFrom(review);
            return retVal;
        }
    }
}
