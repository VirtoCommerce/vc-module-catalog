using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class AssociationServiceImpl : ServiceBase, IAssociationService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        public AssociationServiceImpl(Func<ICatalogRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
        #region IAssociationService members
        public void LoadAssociations(IHasAssociations[] owners)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var productEntities = repository.GetItemByIds(owners.Select(x => x.Id).ToArray(), ItemResponseGroup.ItemAssociations);
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

        public void LoadReferencedAssociations(IHasReferencedAssociations[] owners)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var productIds = owners.Select(o => o.Id);
                var referencedAssociationEntities = repository.Associations.Where(a => productIds.Contains(a.AssociatedItemId));
                var referencedEntityIds = referencedAssociationEntities.Select(a => a.ItemId).Distinct().ToArray();
                foreach (var owner in owners)
                {
                    var referencedItems = repository.GetItemByIds(referencedEntityIds, ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets);
                    var ownerReferencedAssociationEntities = referencedAssociationEntities.Where(a => a.AssociatedItemId == owner.Id).ToList();

                    if (owner.ReferencedAssociations == null)
                    {
                        owner.ReferencedAssociations = new List<ProductAssociation>();
                    }

                    owner.ReferencedAssociations.Clear();
                    foreach (var ownerReferencedAssociationEntity in ownerReferencedAssociationEntities)
                    {
                        var referencedAssociation = ownerReferencedAssociationEntity.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance());
                        var associatedObjectEntity = repository.Items.FirstOrDefault(i => i.Id == ownerReferencedAssociationEntity.ItemId);
                        referencedAssociation.AssociatedObject = associatedObjectEntity;
                        referencedAssociation.AssociatedObjectId = associatedObjectEntity?.Id;
                        referencedAssociation.AssociatedObjectType = "product";

                        owner.ReferencedAssociations.Add(referencedAssociation);
                    }
                }
            }
        }

        public void SaveChanges(IHasAssociations[] owners)
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
            using (var changeTracker = GetChangeTracker(repository))
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var itemIds = owners.Where(x => x.Id != null).Select(x => x.Id).ToArray();
                var existEntities = repository.Associations.Where(x => itemIds.Contains(x.Id)).ToArray();

                var target = new { Associations = new ObservableCollection<AssociationEntity>(existEntities) };
                var source = new { Associations = new ObservableCollection<AssociationEntity>(changedEntities) };

                changeTracker.Attach(target);
                var associationComparer = AnonymousComparer.Create((AssociationEntity x) => x.ItemId + ":" + x.AssociationType + ":" + x.AssociatedItemId + ":" + x.AssociatedCategoryId);
                source.Associations.Patch(target.Associations, associationComparer, (sourceAssociation, targetAssociation) => sourceAssociation.Patch(targetAssociation));

                ((System.Data.Entity.DbContext)repository).ChangeTracker.DetectChanges();
                CommitChanges(repository);
            }

        }
        #endregion

    }
}
