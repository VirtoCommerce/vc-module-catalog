using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class VideoService2 : VideoService
    {
        public VideoService2(
            IOptions<VideoOptions> videoOptions,
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(videoOptions, repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task AfterDeleteAsync(IEnumerable<Video> models, IEnumerable<GenericChangedEntry<Video>> changedEntries)
        {
            return base.AfterDeleteAsync(models, changedEntries);
        }

        protected override Task AfterSaveChangesAsync(IEnumerable<Video> models, IEnumerable<GenericChangedEntry<Video>> changedEntries)
        {
            return base.AfterSaveChangesAsync(models, changedEntries);
        }

        protected override Task BeforeSaveChanges(IEnumerable<Video> models)
        {
            return base.BeforeSaveChanges(models);
        }

        protected override void ClearCache(IEnumerable<Video> models)
        {
            base.ClearCache(models);
        }

        protected override IChangeToken CreateCacheToken(IEnumerable<string> ids)
        {
            return base.CreateCacheToken(ids);
        }

        public override Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        protected override GenericChangedEntryEvent<Video> EventFactory<TEvent>(IEnumerable<GenericChangedEntry<Video>> changedEntries)
        {
            return base.EventFactory<TEvent>(changedEntries);
        }

        public override Task<Video> GetByIdAsync(string id, string responseGroup = null)
        {
            return base.GetByIdAsync(id, responseGroup);
        }

        public override Task<IEnumerable<Video>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null)
        {
            return base.GetByIdsAsync(ids, responseGroup);
        }

        protected override Task<IEnumerable<VideoEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids)
        {
            return base.LoadEntities(repository, ids);
        }

        protected override Task<IEnumerable<VideoEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return base.LoadEntities(repository, ids, responseGroup);
        }

        protected override Video ProcessModel(string responseGroup, VideoEntity entity, Video model)
        {
            return base.ProcessModel(responseGroup, entity, model);
        }

        public override Task SaveChangesAsync(IEnumerable<Video> models)
        {
            return base.SaveChangesAsync(models);
        }

        protected override Task SoftDelete(IRepository repository, IEnumerable<string> ids)
        {
            return base.SoftDelete(repository, ids);
        }
    }
}
