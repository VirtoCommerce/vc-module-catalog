using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class AssociationServiceImpl : CatalogServiceBase, IAssociationService
    {
        public AssociationServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICacheManager<object> cacheManager)
            :base(catalogRepositoryFactory, cacheManager)
        {
        }
        #region IAssociationService members
        public void LoadAssociations(IHasAssociations[] owners)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                var productEntities = repository.GetItemByIds(owners.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemAssociations);
                var allCategories = base.AllCachedCategories;
                var allCatalogs = base.AllCachedCatalogs;
                foreach (var productEntity in productEntities)
                {
                    var owner = owners.FirstOrDefault(x => x.Id == productEntity.Id);
                    if (owner != null)
                    {
                        if (owner.Associations == null)
                        {
                            owner.Associations = new List<ProductAssociation>();
                        }
                        owner.Associations.Clear();
                        owner.Associations.AddRange(productEntity.Associations.Select(x => x.ToCoreModel(allCatalogs, allCategories)));
                    }
                }
            }
        }

        public void SaveChanges(IHasAssociations[] owners)
        {
            var changedDbAssociations = new List<dataModel.AssociationEntity>(); 
            foreach(var owner in owners)
            {
                if(owner.Associations != null)
                {
                    var dbAssociations = owner.Associations.Select(x => x.ToDataModel()).ToArray();
                    foreach(var dbAssociation in dbAssociations)
                    {
                        dbAssociation.ItemId = owner.Id;
                    }
                    changedDbAssociations.AddRange(dbAssociations);
                }
            }

            using (var repository = base.CatalogRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var itemIds = owners.Where(x => x.Id != null).Select(x => x.Id).ToArray();
                var existDbAssociations = repository.Associations.Where(x => itemIds.Contains(x.Id)).ToArray();

                var target = new { Associations = new ObservableCollection<dataModel.AssociationEntity>(existDbAssociations) };
                var source = new { Associations = new ObservableCollection<dataModel.AssociationEntity>(changedDbAssociations) };

                changeTracker.Attach(target);
                var associationComparer = AnonymousComparer.Create((dataModel.AssociationEntity x) => x.ItemId + ":" + x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                source.Associations.Patch(target.Associations, associationComparer, (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));

                CommitChanges(repository);
            }

        }
        #endregion

    }
}
