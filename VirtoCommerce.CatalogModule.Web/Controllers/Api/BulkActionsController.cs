using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/bulkAction")]
    public class BulkActionsController : CatalogBaseController
    {
        private readonly IUserNameResolver _userNameResolver;

        public BulkActionsController(ISecurityService securityService,
            IPermissionScopeService permissionScopeService,
            IUserNameResolver userNameResolver)
            : base(securityService, permissionScopeService)
        {
            _userNameResolver = userNameResolver;
        }

        /// <summary>
        /// Gets the list of available bulk actions
        /// </summary>
        /// <returns>The list of actions</returns>
        [HttpGet]
        [Route("actions")]
        [ResponseType(typeof(IBulkUpdateActionDefinition))]
        [CheckPermission(Permission = CatalogPredefinedPermissions.Access)]
        public IHttpActionResult GetExportedKnownTypes()
        {
            return Ok();
        }


        [HttpPost]
        [Route("actionData")]
        [ResponseType(typeof(IBulkUpdateActionData))]
        [CheckPermission(Permission = CatalogPredefinedPermissions.Access)]
        public IHttpActionResult GetData([FromBody]IBulkUpdateActionContext context)
        {

            return Ok();
        }

        /// <summary>
        /// Starts bulk task
        /// </summary>
        /// <param name="bulkActionContext">Action task description</param>
        /// <returns>Action task id</returns>
        [HttpPost]
        [Route("run")]
        [CheckPermission(Permission = CatalogPredefinedPermissions.BulkActions)]
        public IHttpActionResult RunExport([FromBody]IBulkUpdateActionContext bulkActionContext)
        {

            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = $"{bulkActionContext.ActionName} export task",
                Description = "starting export...."
            };

            //var jobId = BackgroundJob.Enqueue<BulkActionJob>(x => x.ExportBackground(request, notification, JobCancellationToken.Null, null));
            //notification.JobId = jobId;

            return Ok(notification);
        }

        /// <summary>
        /// Attempts to cancel bulk action task
        /// </summary>
        /// <param name="cancellationRequest">Cancellation request with task id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("task/cancel")]
        [CheckPermission(Permission = CatalogPredefinedPermissions.BulkActions)]
        public IHttpActionResult CancelExport([FromBody] BulkActionCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }
    }
}
