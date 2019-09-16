using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionDefinitionBuilder
    {
        public BulkUpdateActionDefinition BulkUpdateActionDefinition { get; private set; }

        /// <summary>
        /// Creates <see cref="ExportedTypeDefinitionBuilder"/> with ExportedTypeDefinition instance.
        /// </summary>
        /// <param name="definition">Definition to build.</param>
        public BulkUpdateActionDefinitionBuilder(BulkUpdateActionDefinition definition)
        {
            BulkUpdateActionDefinition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public static implicit operator BulkUpdateActionDefinition(BulkUpdateActionDefinitionBuilder builder)
        {
            return builder.BulkUpdateActionDefinition;
        }
    }
}
