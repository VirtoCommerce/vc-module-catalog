using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Extensions
{
    public static class BulkUpdateActionDefinitionBuilderExtensions
    {
        public static BulkUpdateActionDefinitionBuilder WithDataSourceFactory(this BulkUpdateActionDefinitionBuilder builder, IPagedDataSourceFactory factory)
        {
            builder.BulkUpdateActionDefinition.DataSourceFactory = factory;
            return builder;
        }

        public static BulkUpdateActionDefinitionBuilder WithActionFactory(this BulkUpdateActionDefinitionBuilder builder, IBulkUpdateActionFactory factory)
        {
            builder.BulkUpdateActionDefinition.Factory = factory;
            return builder;
        }

    }
}
