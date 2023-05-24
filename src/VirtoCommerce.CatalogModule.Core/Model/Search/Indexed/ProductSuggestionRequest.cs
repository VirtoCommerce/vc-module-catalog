namespace VirtoCommerce.CatalogModule.Core.Model.Search.Indexed
{
    public class ProductSuggestionRequest
    {
        /// <summary>
        /// Filter the suggestion results by a catalog ID
        /// </summary>
        public string CatalogId { get; set; }

        /// <summary>
        /// A word, phrase, or text fragment which will be used to make suggestions
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Number of suggestions to return
        /// </summary>
        public int Size { get; set; }
    }
}
