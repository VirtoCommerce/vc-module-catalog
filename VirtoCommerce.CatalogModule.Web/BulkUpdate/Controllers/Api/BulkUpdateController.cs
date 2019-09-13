using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogModule.Data.BulkUpdate;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.BulkUpdate.Model;
using VirtoCommerce.ExportModule.Core.Model;
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
        [ResponseType(typeof(IBulkUpdateActionDefinition[]))]
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
                ?? throw new ArgumentException($"Export type \"{context.ActionName}\" is not registered using \"{nameof(IBulkUpdateActionRegistrar)}\".");

            if (!Authorize(actionDefinition, context))
            {
                return Unauthorized();
            }

            var actionFactory = actionDefinition.Factory;
            var action = actionFactory.Create(context);

            return Ok(action.GetActionData());
        }

        /// <summary>
        /// Starts bulk update task task
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <returns>Export task id</returns>
        [HttpPost]
        [Route("run")]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Execute)]
        //[ResponseType(typeof(PlatformExportPushNotification))]
        public IHttpActionResult RunExport([FromBody]BulkUpdateActionContext context)
        {
            //var exportedTypeDefinition = _knownExportTypesResolver.ResolveExportedTypeDefinition(request.ExportTypeName)
            //    ?? throw new ArgumentException($"Export type \"{request.ExportTypeName}\" is not registered using \"{nameof(IKnownExportTypesRegistrar)}\".");

            //if (!Authorize(actionDefinition, context))
            //{
            //    return Unauthorized();
            //}

            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = $"{context.ActionName} export task",
                Description = "starting export...."
            };

            //var jobId = BackgroundJob.Enqueue<ExportJob>(x => x.ExportBackground(request, notification, JobCancellationToken.Null, null));
            //notification.JobId = jobId;

            return Ok(notification);
        }

        /// <summary>
        /// Attempts to cancel export task
        /// </summary>
        /// <param name="cancellationRequest">Cancellation request with task id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("task/cancel")]
        [CheckPermission(Permission = BulkUpdatePredefinedPermissions.Execute)]
        public IHttpActionResult CancelExport([FromBody]UpdateCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }

        #region Authorization

        /// <summary>
        /// 
        /// Performs all definition security handlers checks, and returns true if all are succeeded.
        /// </summary>
        /// <param name="definition">ExportedTypeDefinition.</param>
        /// <param name="context">ExportDataRequest.</param>
        /// <returns>True if all checks are succeeded, otherwise false.</returns>
        private bool Authorize(IBulkUpdateActionDefinition definition, BulkUpdateActionContext context)
        {
            // TechDebt: Need to add permission and custom authorization for bulk update.
            // For that we could use IExportSecurityHandler and IPerrmissionExportSecurityHandlerFactory - just need to move them to platform and remove export specific objects
            return true;
        }

        #endregion Authorization
    }

}
