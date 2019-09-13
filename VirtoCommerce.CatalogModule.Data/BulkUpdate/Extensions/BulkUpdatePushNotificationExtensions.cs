using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Extensions
{
    public static class BulkUpdatePushNotificationExtensions
    {
        public static void Patch(this BulkUpdatePushNotification target, BulkUpdateProgressInfo source)
        {
            target.Description = source.Description;
            target.Errors = source.Errors;
            target.ProcessedCount = source.ProcessedCount;
            target.TotalCount = source.TotalCount;
        }
    }
}
