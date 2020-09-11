using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
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
                var itemIds = owners.Where(x => x.Id != null).Select(x => x.Id).ToArray();
                var existEntities = repository.Associations.Where(x => itemIds.Contains(x.ItemId)).ToList();

                var associationComparer = AnonymousComparer.Create((AssociationEntity x) => x.ItemId + ":" + x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                changedEntities.Patch(existEntities, associationComparer, (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));

                await repository.UnitOfWork.CommitAsync();
            }
        }

        public async Task UpdateAssociationsAsync(ProductAssociation[] associations)
        {
            var changedEntries = new List<GenericChangedEntry<ProductAssociation>>();

            using (var repository = _repositoryFactory())
            {
                var ids = associations.Where(x => !x.IsTransient()).Select(x => x.Id);
                var dbExistsAssociation = repository.Associations.Where(x => ids.Contains(x.Id));

                foreach (var association in associations)
                {
                    var modifiedEntity = AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(association);
                    var originalEntity = dbExistsAssociation.FirstOrDefault(x => x.Id == association.Id);

                    if (originalEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<ProductAssociation>(association, originalEntity.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<ProductAssociation>(association, EntryState.Added));
                    }
                }

                await repository.UnitOfWork.CommitAsync();

                ClearCache(dbExistsAssociation.Select(x => x.ItemId).ToArray());
            }

            //Reset cached associations

            ClearCache(associations.Select(x => x.ItemId).ToArray());
        }

        public async Task DeleteAssociationAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                var associations = repository.Associations.Where(x => ids.Contains(x.Id)).ToArray();

                foreach (var association in associations)
                {
                    repository.Remove(association);
                }

                await repository.UnitOfWork.CommitAsync();

                ClearCache(associations.Select(x => x.ItemId).ToArray());
            }
        }

        private AssociationEntity[] GetAssociationsByIdsAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var result = repository.Associations
                    .Where(x => ids.Contains(x.Id));

                return result.ToArray();
            }
        }

        #endregion

        private void ClearCache(string[] ids)
        {
            ItemCacheRegion.ExpireProducts(ids);
            AssociationSearchCacheRegion.ExpireRegion();
        }
    }
}
