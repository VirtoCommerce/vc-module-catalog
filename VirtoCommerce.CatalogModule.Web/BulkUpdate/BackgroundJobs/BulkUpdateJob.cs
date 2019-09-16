using System;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Extensions;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Data.Common;

namespace VirtoCommerce.CatalogModule.Web.BulkUpdate.BackgroundJobs
{
    public class BulkUpdateJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IBulkUpdateActionExecutor _bulkUpdateActionExecutor;

        public BulkUpdateJob(IBulkUpdateActionRegistrar bulkUpdateActionRegistrar,
            IPushNotificationManager pushNotificationManager,
            IBulkUpdateActionExecutor bulkUpdateActionExecutor)
        {
            _pushNotificationManager = pushNotificationManager;
            _bulkUpdateActionExecutor = bulkUpdateActionExecutor;
        }

        public void Execute(BulkUpdateActionContext bulkUpdateActionContext, BulkUpdatePushNotification notification, IJobCancellationToken cancellationToken, PerformContext performContext)
        {
            if (bulkUpdateActionContext == null)
            {
                throw new ArgumentNullException(nameof(bulkUpdateActionContext));
            }

            if (performContext == null)
            {
                throw new ArgumentNullException(nameof(performContext));
            }

            void progressCallback(BulkUpdateProgressInfo x)
            {
                notification.Patch(x);
                notification.JobId = performContext.BackgroundJob.Id;
                _pushNotificationManager.Upsert(notification);
            }

            try
            {
                _bulkUpdateActionExecutor.Execute(bulkUpdateActionContext, progressCallback, new JobCancellationTokenWrapper(cancellationToken));
            }
            catch (JobAbortedException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                notification.Errors.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                notification.Description = "Update finished";
                notification.Finished = DateTime.UtcNow;
                _pushNotificationManager.Upsert(notification);
            }
        }
    }
}
