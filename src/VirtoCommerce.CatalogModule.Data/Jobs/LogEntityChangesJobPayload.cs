using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that persists catalog change-log entries.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
