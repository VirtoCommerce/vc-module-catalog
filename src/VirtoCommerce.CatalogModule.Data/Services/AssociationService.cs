using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class AssociationService : IAssociationService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        public AssociationService(Func<ICatalogRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        #region IAssociationService members
        public async Task LoadAssociationsAsync(IHasAssociations[] owners)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var productEntities = await repository.GetItemByIdsAsync(owners.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemAssociations.ToString());
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
                        owner.Associations.AddRange(productEntity.Associations.Select(x => x.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())));
                    }
                }
            }
        }

        public async Task<ProductAssociation[]> GetAssociationsAsync(string[] ownerIds)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var productEntities = await repository.GetItemByIdsAsync(ownerIds, ItemResponseGroup.ItemAssociations.ToString());
                return productEntities.SelectMany(x => x.Associations)
                    .Select(x => x.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance())).ToArray();
            }
        }

        public async Task SaveChangesAsync(IHasAssociations[] owners)
        {
            var changedEntities = new List<AssociationEntity>();
            foreach (var owner in owners)
            {
                if (owner.Associations != null)
                {
                    var dbAssociations = owner.Associations.Select(x => AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(x)).ToArray();
                    foreach (var dbAssociation in dbAssociations)
                    {
                        dbAssociation.ItemId = owner.Id;
                    }
                    changedEntities.AddRange(dbAssociations);
                }
            }

            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var itemIds = owners.Where(x => x.Id != null).Select(x => x.Id).ToArray();
                var existEntities = await repository.Associations.Where(x => itemIds.Contains(x.ItemId)).ToArrayAsync();

                var target = new { Associations = new ObservableCollection<AssociationEntity>(existEntities) };
                var source = new { Associations = new ObservableCollection<AssociationEntity>(changedEntities) };

                //changeTracker.Attach(target);
                var associationComparer = AnonymousComparer.Create((AssociationEntity x) => x.ItemId + ":" + x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                source.Associations.Patch(target.Associations, associationComparer, (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));

                await repository.UnitOfWork.CommitAsync();
                //Reset cached associations
                ItemCacheRegion.ExpireRegion();
                AssociationSearchCacheRegion.ExpireRegion();
            }

        }

        public async Task UpdateAssociationsAsync(ProductAssociation[] associations)
        {
            var changedEntities = new List<AssociationEntity>();

            var dbAssociations = associations.Select(x => AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(x)).ToArray();

            changedEntities.AddRange(dbAssociations);

            using (var repository = _repositoryFactory())
            {
                var changedProducts = new List<CatalogProduct>();
                foreach (var changedEntity in changedEntities)
                {
                    var existEntity = repository.Associations.FirstOrDefault(x => changedEntity.ItemId == x.ItemId && changedEntity.AssociatedItemId == x.AssociatedItemId);

                    var products = await repository.GetItemByIdsAsync(new[] { changedEntity.ItemId });
                    var product = products.Select(x => x.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance(), false, false)).First();
                    changedProducts.AddDistinct(product);

                    if (existEntity == null)
                        repository.Add(changedEntity);
                    else
                        changedEntity.Patch(existEntity);

                }
                await repository.UnitOfWork.CommitAsync();

                //Reset cached associations

                foreach (var product in changedProducts)
                    ItemCacheRegion.ExpireEntity(product);

                AssociationSearchCacheRegion.ExpireRegion();
            }
        }

        public async Task DeleteAssociationAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.RemoveAssociationsAsync(ids);
                //Reset cached associations
                AssociationSearchCacheRegion.ExpireRegion();
            }
        }
        #endregion

    }
}
