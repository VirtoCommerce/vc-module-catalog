using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogModule.Data.Repositories;

namespace VirtoCommerce.CatalogModule.Data.MySql
{
    public class MySqlDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CatalogDbContext>();
            var connectionString = args.Any() ? args[0] : "server=localhost;user=root;password=virto;database=VirtoCommerce3;";
            var serverVersion = args.Length >= 2 ? args[1] : null;

            builder.UseMySql(
                connectionString,
                ResolveServerVersion(serverVersion, connectionString),
                db => db
                    .MigrationsAssembly(typeof(MySqlDataAssemblyMarker).Assembly.GetName().Name));

            return new CatalogDbContext(builder.Options);
        }

        private static ServerVersion ResolveServerVersion(string serverVersion, string connectionString)
        {
            if (serverVersion == "AutoDetect")
            {
                return ServerVersion.AutoDetect(connectionString);
            }
            else if (serverVersion != null)
            {
                return ServerVersion.Parse(serverVersion);
            }
            return new MySqlServerVersion(new Version(5, 7));
        }
    }
}
