namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class CategoryPropertyValidationRequest
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public PropertyValueType PropertyValueType { get; set; }

        public string CategoryId { get; set; }
        public string CatalogId { get; set; }
    }
}
