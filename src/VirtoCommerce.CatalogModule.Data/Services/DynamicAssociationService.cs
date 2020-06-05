using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationService : IDynamicAssociationService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;

        public DynamicAssociationService(Func<ICatalogRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<DynamicAssociation[]> GetByIdsAsync(string[] itemIds)
        {
            //ToDo: Use Cache
            var rules = Array.Empty<DynamicAssociation>();

            if (!itemIds.IsNullOrEmpty())
            {
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var entities = await repository.DynamicAssociations.Where(x => itemIds.Contains(x.Id)).ToArrayAsync();

                    rules = entities
                        .Select(x => x.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()))
                        .ToArray();
                }
            }

            return rules;
        }

        public async Task SaveChangesAsync(DynamicAssociation[] items)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<DynamicAssociation>>();

            using (var repository = _repositoryFactory())
            {
                var ids = items.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray();
                var dbExistProducts = await repository.DynamicAssociations
                    .Where(x => ids.Contains(x.Id))
                    .ToArrayAsync();
                foreach (var dynamicAssociation in items)
                {
                    var modifiedEntity = AbstractTypeFactory<DynamicAssociationEntity>.TryCreateInstance().FromModel(dynamicAssociation, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == dynamicAssociation.Id);

                    if (originalEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, originalEntity.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, EntryState.Added));
                    }
                }

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                //ToDo: Event publish
            }

            //ToDo: Clear cache
        }

        public Task DeleteAsync(string[] itemIds)
        {
            throw new System.NotImplementedException();
        }
    }
}
