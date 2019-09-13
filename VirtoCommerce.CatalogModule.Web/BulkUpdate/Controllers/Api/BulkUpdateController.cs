using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogModule.Data.BulkUpdate;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.BulkUpdate.BackgroundJobs;
using VirtoCommerce.CatalogModule.Web.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.CatalogModule.Web.BulkUpdate.Controllers.Api
{
    [RoutePrefix("api/bulkUpdate")]
    public class BulkUpdateController : ApiController
    {
        private readonly IBulkUpdateActionRegistrar _bulkUpdateActionRegistrar;
        private readonly IUserNameResolver _userNameResolver;

        public BulkUpdateController(
            IBulkUpdateActionRegistrar bulkUpdateActionRegistrar,
            IUserNameResolver userNameResolver)
        {
            _bulkUpdateActionRegistrar = bulkUpdateActionRegistrar;
            _userNameResolver = userNameResolver;
        }

        /// <summary>
        /// Gets the list of all registered actions
        /// </summary>
        /// <returns>The list of registered actions</returns>
        [HttpGet]
        [Route("actions")]
        [ResponseType(typeof(BulkUpdateActionDefinition[]))]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Read)]
        public IHttpActionResult GetRegisteredActions()
        {
            return Ok(_bulkUpdateActionRegistrar.GetAll().ToArray());
        }

        /// <summary>
        /// Gets action initialization data (could be used to initialize UI).
        /// </summary>
        /// <param name="context">Context for which we want initialization data.</param>
        /// <returns>Initialization data for the given context.</returns>
        [HttpPost]
        [Route("action/data")]
        [ResponseType(typeof(IBulkUpdateActionData))]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Read)]
        public IHttpActionResult GetActionData([FromBody]BulkUpdateActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var actionDefinition = _bulkUpdateActionRegistrar.GetByName(context.ActionName)
                ?? throw new ArgumentException($"Action \"{context.ActionName}\" is not registered using \"{nameof(IBulkUpdateActionRegistrar)}\".");

            if (!Authorize(actionDefinition, context))
            {
                return Unauthorized();
            }

            var actionFactory = actionDefinition.Factory;
            var action = actionFactory.Create(context);

            return Ok(action.GetActionData());
        }

        /// <summary>
        /// Starts bulk update task task.
        /// </summary>
        /// <param name="context">Execution context.</param>
        /// <returns>Notification with job id.</returns>
        [HttpPost]
        [Route("run")]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Execute)]
        [ResponseType(typeof(BulkUpdatePushNotification))]
        public IHttpActionResult Run([FromBody]BulkUpdateActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var actionDefinition = _bulkUpdateActionRegistrar.GetByName(context.ActionName)
                ?? throw new ArgumentException($"Action \"{context.ActionName}\" is not registered using \"{nameof(IBulkUpdateActionRegistrar)}\".");

            if (!Authorize(actionDefinition, context))
            {
                return Unauthorized();
            }

            var notification = new BulkUpdatePushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = $"{context.ActionName}",
                Description = "Starting…"
            };

            var jobId = BackgroundJob.Enqueue<BulkUpdateJob>(x => x.Execute(context, notification, JobCancellationToken.Null, null));
            notification.JobId = jobId;

            return Ok(notification);
        }

        /// <summary>
        /// Attempts to cancel running task
        /// </summary>
        /// <param name="cancellationRequest">Cancellation request with task id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("task/cancel")]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Execute)]
        public IHttpActionResult Cancel([FromBody]UpdateCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }

        #region Authorization

        /// <summary>
        /// Performs all definition security handlers checks, and returns true if all are succeeded.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="context"></param>
        /// <returns>True if all checks are succeeded, otherwise false.</returns>
        private bool Authorize(BulkUpdateActionDefinition definition, BulkUpdateActionContext context)
        {
            // TechDebt: Need to add permission and custom authorization for bulk update.
            // For that we could use IExportSecurityHandler and IPerrmissionExportSecurityHandlerFactory - just need to move them to platform and remove export specific objects
            return true;
        }

        #endregion Authorization
    }

}
