namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background jobs that rebuild or drop a category's automatic links. One payload type serves both
    /// jobs: the work is addressed by handler, and both need exactly the category id.
    /// </summary>
    public class AutomaticLinksJobPayload
    {
        public string CategoryId { get; set; }
    }
}
