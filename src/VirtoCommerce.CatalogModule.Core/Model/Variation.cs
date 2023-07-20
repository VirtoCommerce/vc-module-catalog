namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Variation : CatalogProduct
    {
        protected override void InheritReviews(CatalogProduct parentProduct)
        {
            // Do not inherit editorial reviews from main product if variation loaded within product
        }
    }
}
