namespace VirtoCommerce.CatalogModule.Core.Model.Search
{
    public class Aggregation
    {
        /// <summary>
        /// Gets or sets the value of the aggregation type
        /// </summary>
        /// <value>
        /// "Attribute", "PriceRange", "Range" or "Category"
        /// </value>
        public string AggregationType { get; set; }

        /// <summary>
        /// Gets or sets the value of the aggregation field
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the collection of the aggregation labels
        /// </summary>
        public AggregationLabel[] Labels { get; set; }

        /// <summary>
        /// Gets or sets the collection of the aggregation items
        /// </summary>
        public AggregationItem[] Items { get; set; }
    }
}
