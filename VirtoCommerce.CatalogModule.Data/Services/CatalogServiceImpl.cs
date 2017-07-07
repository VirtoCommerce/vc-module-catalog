using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogServiceImpl : ServiceBase, ICatalogService
    {
        private readonly ICommerceService _commerceService;
        private readonly ICacheManager<object> _cacheManager;
        private readonly IPropertyValueValidator _propertyValuesValidator;
        private readonly Func<ICatalogRepository> _repositoryFactory;

        public CatalogServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICommerceService commerceService, ICacheManager<object> cacheManager,
            IPropertyValueValidator propertyValuesValidator)
        {
            _commerceService = commerceService;
            _repositoryFactory = catalogRepositoryFactory;
            _cacheManager = cacheManager;
            _propertyValuesValidator = propertyValuesValidator;
        }

        #region ICatalogService Members

        public Catalog GetById(string catalogId)
        {
            //Clone required because client code may change resulting objects
            var result = PreloadCatalogs().Where(x => x.Id == catalogId).Select(x => MemberwiseCloneCatalog(x)).FirstOrDefault();
            return result;
        }

        public Catalog Create(Catalog catalog)
        {
            SaveChanges(new[] { catalog });
            var result = GetById(catalog.Id);
            return result;
        }

        public void Update(Catalog[] catalogs)
        {
            SaveChanges(catalogs);
        }

        public void Delete(string[] catalogIds)
        {
            using (var repository = _repositoryFactory())
            {
                repository.RemoveCatalogs(catalogIds);
                CommitChanges(repository);
                //Reset cached catalogs and catalogs
                ResetCache();
            }
        }

        public IEnumerable<Catalog> GetCatalogsList()
        {
            //Clone required because client code may change resulting objects
            return PreloadCatalogs().Select(x => MemberwiseCloneCatalog(x)).OrderBy(x => x.Name);
        }

        #endregion

        protected virtual void SaveChanges(Catalog[] catalogs)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                ValidateCatalogProperties(catalogs);
                var dbExistEntities = repository.GetCatalogsByIds(catalogs.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var catalog in catalogs)
                {
                    var originalEntity = dbExistEntities.FirstOrDefault(x => x.Id == catalog.Id);
                    var modifiedEntity = AbstractTypeFactory<CatalogEntity>.TryCreateInstance().FromModel(catalog, pkMap);
                    if (originalEntity != null)
                    {
                        changeTracker.Attach(originalEntity);
                        modifiedEntity.Patch(originalEntity);                     
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
                //Reset cached catalogs and catalogs
                ResetCache();
            }          
        }

        protected virtual void ResetCache()
        {
            _cacheManager.ClearRegion(CatalogConstants.CacheRegion);
        }

       
        protected virtual Catalog MemberwiseCloneCatalog(Catalog catalog)
        {
            var retVal = AbstractTypeFactory<Catalog>.TryCreateInstance();
            retVal.Id = catalog.Id;
            retVal.IsVirtual = catalog.IsVirtual;
            retVal.Name = catalog.Name;
            retVal.Properties = catalog.Properties;
            retVal.Languages = catalog.Languages;
            retVal.PropertyValues = catalog.PropertyValues;
            return retVal;
        }

        protected virtual Catalog[] PreloadCatalogs()
        {
            return _cacheManager.Get("AllCatalogs", CatalogConstants.CacheRegion, () =>
            {
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    return repository.GetCatalogsByIds(repository.Catalogs.Select(x => x.Id).ToArray())
                                      .Select(x => x.ToModel(AbstractTypeFactory<Catalog>.TryCreateInstance()))
                                      .ToArray();
                }
            });
        }

        private void ValidateCatalogProperties(Catalog[] catalogs)
        {
            var allErrors = new List<string>();
            var preloadedCategories = catalogs.Select(x => GetById(x.Id));
            var preloadedProperties = preloadedCategories.SelectMany(x => x.Properties);
            var rulesDictionary = new Dictionary<string, List<PropertyValidationRule>>();

            foreach (var property in preloadedProperties)
            {
                if (!rulesDictionary.ContainsKey(property.Id) && property.ValidationRules.Any())
                    rulesDictionary.Add(property.Id, property.ValidationRules.ToList());
            }

            foreach (var propValue in catalogs.SelectMany(x => x.PropertyValues))
            {
                var rules = new List<PropertyValidationRule>();
                if (rulesDictionary.ContainsKey(propValue.PropertyId))
                    rules = rulesDictionary[propValue.PropertyId];

                var errors = new List<string>();
                rules.ForEach(rule => { errors.AddRange(_propertyValuesValidator.Validate(rule, propValue)); });
                allErrors.AddRange(errors.FormatPropertyErrors(propValue));
            }

            if (allErrors.Any())
                throw new Exception($"Catalog properties has validation error: {string.Join(Environment.NewLine, allErrors)}");
        }
    }
}
