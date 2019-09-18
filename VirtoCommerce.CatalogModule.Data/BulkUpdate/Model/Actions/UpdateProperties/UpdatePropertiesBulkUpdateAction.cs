using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesBulkUpdateAction : IBulkUpdateAction
    {
        private readonly UpdatePropertiesActionContext _context;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;

        public UpdatePropertiesBulkUpdateAction(IBulkUpdatePropertyManager bulkUpdatePropertyManager, UpdatePropertiesActionContext context)
        {
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context => _context;

        public IBulkUpdateActionData GetActionData()
        {
            //var dataQuery = _context.DataQuery as ProductBulkUpdateDataQuery ?? throw new InvalidCastException($"{nameof(Context)}.{nameof(UpdatePropertiesActionContext.DataQuery)} should be of \"{nameof(ProductBulkUpdateDataQuery)}\" type.");

            var properties = _bulkUpdatePropertyManager.GetProperties(_context);

            return new UpdatePropertiesActionData()
            {
                Properties = properties.Select(x => x.ToWebModel()).ToArray(),
            };
        }

        public BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;

            return result;
        }

        public BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            throw new NotImplementedException();
        }

    }
}
