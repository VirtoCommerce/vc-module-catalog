using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Web.Security
{
    public class CatalogSelectedScope : PermissionScope
    {

        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == CatalogPredefinedPermissions.Read
                      || permission == CatalogPredefinedPermissions.Update;
        }

        public override IEnumerable<string> GetEntityScopeStrings(object entity)
        {
            if (entity == null) { throw new ArgumentNullException(nameof(entity)); }

            var catalogId = GetCatalogIdFromEntity(entity);

            return catalogId == null ? Enumerable.Empty<string>() : new[] { $"{Type}:{catalogId}" };
        }

        private string GetCatalogIdFromEntity(object entity)
        {
            switch (entity)
            {
                case Catalog catalog:
                    return catalog.Id;
                case Category category:
                    return category.CatalogId;
                case CatalogProduct catalogProduct:
                    return catalogProduct.CatalogId;
                case Model.ListEntryLink listEntryLink:
                    return listEntryLink.CatalogId;
                case Model.Property property:
                    return property.CatalogId;
            }

            return null;
        }
    }
}
