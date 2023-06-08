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

namespace VirtoCommerce.CatalogModule2.Web.Services
{
    public class VideoService2 : VideoService
    {
        public VideoService2(
            IOptions<VideoOptions> videoOptions,
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher, videoOptions)
        {
        }

        protected override Task AfterDeleteAsync(IList<Video> models, IList<GenericChangedEntry<Video>> changedEntries)
        {
            return base.AfterDeleteAsync(models, changedEntries);
        }

        protected override Task AfterSaveChangesAsync(IList<Video> models, IList<GenericChangedEntry<Video>> changedEntries)
        {
            return base.AfterSaveChangesAsync(models, changedEntries);
        }

        protected override Task BeforeSaveChanges(IList<Video> models)
        {
            return base.BeforeSaveChanges(models);
        }

        protected override void ClearCache(IList<Video> models)
        {
            base.ClearCache(models);
        }

        protected override IChangeToken CreateCacheToken(string id)
        {
            return base.CreateCacheToken(id);
        }

        public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        protected override GenericChangedEntryEvent<Video> EventFactory<TEvent>(IList<GenericChangedEntry<Video>> changedEntries)
        {
            return base.EventFactory<TEvent>(changedEntries);
        }

        public override Task<IList<Video>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
        {
            return base.GetAsync(ids, responseGroup, clone);
        }

        protected override Task<IList<VideoEntity>> LoadEntities(IRepository repository, IList<string> ids)
        {
            return base.LoadEntities(repository, ids);
        }

        protected override Task<IList<VideoEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return base.LoadEntities(repository, ids, responseGroup);
        }

        protected override Video ProcessModel(string responseGroup, VideoEntity entity, Video model)
        {
            return base.ProcessModel(responseGroup, entity, model);
        }

        public override Task SaveChangesAsync(IList<Video> models)
        {
            return base.SaveChangesAsync(models);
        }

        protected override Task SoftDelete(IRepository repository, IList<string> ids)
        {
            return base.SoftDelete(repository, ids);
        }
    }
}
