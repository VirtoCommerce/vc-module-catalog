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

        public override IEnumerable<string> GetEntityScopeStrings(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            var catalog = obj as Catalog;
            var category = obj as Category;
            var product = obj as CatalogProduct;
            var link = obj as Model.ListEntryLink;
            var property = obj as Property;

            string catalogId = null;
            if (catalog != null)
                catalogId = catalog.Id;
            if (category != null)
                catalogId = category.CatalogId;
            if (product != null)
                catalogId = product.CatalogId;
            if (link != null)
                catalogId = link.CatalogId;
            if (property != null)
                catalogId = property.CatalogId;

            if (catalogId != null)
            {
                return new[] { Type + ":" + catalogId };
            }
            return Enumerable.Empty<string>();
        }
    }
}
