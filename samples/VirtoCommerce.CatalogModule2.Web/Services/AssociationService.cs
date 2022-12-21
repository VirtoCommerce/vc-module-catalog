using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class AssociationService2 : AssociationService
    {
        public AssociationService2(Func<ICatalogRepository> repositoryFactory) : base(repositoryFactory)
        {
        }
        protected override Task ValidateAssociationAsync(IEnumerable<ProductAssociation> associations)
        {
            return base.ValidateAssociationAsync(associations);
        }
    }
}
