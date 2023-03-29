using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule.Data.SqlServer;

public class ItemEntityConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> builder)
    {
        builder.HasIndex(x => new { x.CreatedDate, x.ParentId }).IncludeProperties(x => x.ModifiedDate).IsUnique(false).HasDatabaseName("IX_CreatedDate_ParentId");
    }
}
