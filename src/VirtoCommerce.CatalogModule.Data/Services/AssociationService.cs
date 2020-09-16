using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Validation;
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
        /// <summary>
        /// Saves collections of associations for given objects. Replaces all collection - adds new, updates existing and deletes associations that are not presented in the saved objects associations.
        /// </summary>
        /// <param name="owners"></param>
        /// <returns></returns>
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
                var existingEntities = repository.Associations.Where(x => itemIds.Contains(x.ItemId)).ToList();

                var associationComparer = AnonymousComparer.Create((AssociationEntity x) => x.ItemId + ":" + x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                changedEntities.Patch(existingEntities, associationComparer, (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation), repository);

                await repository.UnitOfWork.CommitAsync();
            }
        }

        /// <summary>
        /// Saves association entities. Adds new or updates existing based on ids.
        /// </summary>
        /// <param name="associations"></param>
        /// <returns></returns>
        public async Task UpdateAssociationsAsync(ProductAssociation[] associations)
        {

            await ValidateAssociationAsync(associations);

            using (var repository = _repositoryFactory())
            {
                var ids = associations.Where(x => !x.IsTransient()).Select(x => x.Id);
                var existingAssociation = repository.Associations.Where(x => ids.Contains(x.Id)).ToList();
                
                foreach (var association in associations)
                {
                    var modifiedEntity = AbstractTypeFactory<AssociationEntity>.TryCreateInstance().FromModel(association);
                    var originalEntity = existingAssociation.FirstOrDefault(x => x.Id == association.Id);

                    if (originalEntity != null)
                    {
                        if (modifiedEntity.ItemId != originalEntity.ItemId)
                        {
                            //Reset cache for item that has lost association 
                            ClearCache(new[] { originalEntity.ItemId });
                        }

                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();
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

        #endregion

        private void ClearCache(string[] ids)
        {
            ItemCacheRegion.ExpireProducts(ids);
            AssociationSearchCacheRegion.ExpireRegion();
        }

        protected virtual async Task ValidateAssociationAsync(IEnumerable<ProductAssociation> associations)
        {
            if (associations == null)
            {
                throw new ArgumentNullException(nameof(associations));
            }

            var validator = new ProductAssociationValidator();
            foreach (var association in associations)
            {
                await validator.ValidateAndThrowAsync(association);
            }

        }
    }
}
