using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class PropertyService2 : PropertyService
    {
        public PropertyService2(Func<ICatalogRepository> repositoryFactory,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            ICatalogSearchService catalogSearchService,
            AbstractValidator<Property> propertyValidator) : base(
                repositoryFactory,
                eventPublisher,
                platformMemoryCache,
                catalogSearchService,
                propertyValidator
                )
        {
        }

        protected override void ApplyInheritanceRules(Property[] properties)
        {
            base.ApplyInheritanceRules(properties);
        }
        protected override Task LoadDependenciesAsync(Property[] properties)
        {
            return base.LoadDependenciesAsync(properties);
        }
        protected override Task<Property[]> PreloadAllCatalogPropertiesAsync(string catalogId)
        {
            return base.PreloadAllCatalogPropertiesAsync(catalogId);
        }
        protected override Task<IDictionary<string, Property>> PreloadAllPropertiesAsync()
        {
            return base.PreloadAllPropertiesAsync();
        }
        protected override void TryAddPredefinedValidationRules(IEnumerable<Property> properties)
        {
            base.TryAddPredefinedValidationRules(properties);
        }
        protected override void ValidateProperties(IEnumerable<Property> properties)
        {
            base.ValidateProperties(properties);
        }
    }
}
