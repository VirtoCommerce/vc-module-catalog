using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.MySql
{
    public class MySqlCatalogRawDatabaseCommand : ICatalogRawDatabaseCommand
    {
        private const int BatchSize = 500;

        public async Task<string[]> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, string[] categoryIds)
        {
            if (categoryIds.Length == 0)
            {
                return Array.Empty<string>();
            }

            var result = new HashSet<string>();

            var nextLevelCategories = new HashSet<string>(categoryIds);

            var allChildCategories = await dbContext.Set<CategoryEntity>()
                .Where(item => item.ParentCategoryId != null)
                .Select(item => new {item.Id, item.ParentCategoryId})
                .ToListAsync();

            var parentLookup = allChildCategories.ToLookup(item => item.ParentCategoryId, item => item.Id);

            IEnumerable<string> GetNextLevelResult(ISet<string> ids)
            {
                foreach (var id in ids)
                {
                    foreach (var childId in parentLookup[id])
                    {
                        yield return childId;
                    }
                }
            }

            while (nextLevelCategories.Count > 0)
            {
                var nextLevelResult = new HashSet<string>(GetNextLevelResult(nextLevelCategories));

                result.UnionWith(nextLevelResult);
                nextLevelCategories.Clear();
                nextLevelCategories.UnionWith(nextLevelResult);
            }

            return result.ToArray();
        }

        public Task<string[]> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext)
        {
            throw new NotImplementedException();
        }

        public Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId)
        {
            var commandTemplate = @"
                     SELECT t2.* FROM (
                     SELECT @r AS _id,
                          (SELECT @r := ParentCategoryId FROM Category WHERE Id = _id) AS _parent_id,
                          @l := @l + 1 AS level
                     FROM (SELECT @r := @categoryId, @l := 0) val, Category
                     WHERE @r IS NOT NULL ) t1
                     JOIN Category t2
                     ON t1._id = t2.Id";

            var categoryIdParam = new MySqlParameter("@categoryId", categoryId);
            var result = await dbContext.Set<CategoryEntity>().FromSqlRaw(commandTemplate, categoryIdParam).ToListAsync();

            return result;
        }

        public async Task RemoveAllPropertyValuesAsync(CatalogDbContext dbContext, PropertyEntity catalogProperty, PropertyEntity categoryProperty, PropertyEntity itemProperty)
        {
            FormattableString commandText;
            if (catalogProperty != null)
            {
                commandText = $"DELETE PV FROM PropertyValue PV INNER JOIN Catalog C ON C.Id = PV.CatalogId AND C.Id = '{catalogProperty.CatalogId}' WHERE PV.Name = '{catalogProperty.Name}'";
                await dbContext.Database.ExecuteSqlInterpolatedAsync(commandText);
            }
            if (categoryProperty != null)
            {
                commandText = $"DELETE PV FROM PropertyValue PV INNER JOIN Category C ON C.Id = PV.CategoryId AND C.CatalogId = '{categoryProperty.CatalogId}' WHERE PV.Name = '{categoryProperty.Name}'";
                await dbContext.Database.ExecuteSqlInterpolatedAsync(commandText);
            }
            if (itemProperty != null)
            {
                commandText = $"DELETE PV FROM PropertyValue PV INNER JOIN Item I ON I.Id = PV.ItemId AND I.CatalogId = '{itemProperty.CatalogId}' WHERE PV.Name = '{itemProperty.Name}'";
                await dbContext.Database.ExecuteSqlInterpolatedAsync(commandText);
            }

        }

        public async Task RemoveCatalogsAsync(CatalogDbContext dbContext, string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                    DELETE CL FROM CatalogLanguage CL INNER JOIN Catalog C ON C.Id = CL.CatalogId WHERE C.Id IN ({0});
                    DELETE CR FROM CategoryRelation CR INNER JOIN Catalog C ON C.Id = CR.TargetCatalogId WHERE C.Id IN ({0});
                    DELETE PV FROM PropertyValue PV INNER JOIN Catalog C ON C.Id = PV.CatalogId WHERE C.Id IN ({0});
                    DELETE P FROM Property P INNER JOIN Catalog C ON C.Id = P.CatalogId  WHERE C.Id IN ({0});
                    DELETE FROM Catalog WHERE Id IN ({0});
                ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(BatchSize));
                    skip += BatchSize;
                }
                while (skip < ids.Length);
            }

        }

        public async Task RemoveCategoriesAsync(CatalogDbContext dbContext, string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                    DELETE SEO FROM CatalogSeoInfo SEO INNER JOIN Category C ON C.Id = SEO.CategoryId WHERE C.Id IN ({0});
                    DELETE CI FROM CatalogImage CI INNER JOIN Category C ON C.Id = CI.CategoryId WHERE C.Id IN ({0});
                    DELETE PV FROM PropertyValue PV INNER JOIN Category C ON C.Id = PV.CategoryId WHERE C.Id IN ({0});
                    DELETE CR FROM CategoryRelation CR INNER JOIN Category C ON C.Id = CR.SourceCategoryId OR C.Id = CR.TargetCategoryId  WHERE C.Id IN ({0});
                    DELETE CIR FROM CategoryItemRelation CIR INNER JOIN Category C ON C.Id = CIR.CategoryId WHERE C.Id IN ({0});
                    DELETE A FROM Association A INNER JOIN Category C ON C.Id = A.AssociatedCategoryId WHERE C.Id IN ({0});
                    DELETE P FROM Property P INNER JOIN Category C ON C.Id = P.CategoryId  WHERE C.Id IN ({0});
                    DELETE D FROM CategoryDescription D INNER JOIN Category C ON C.Id = D.CategoryId WHERE C.Id IN ({0});
                    DELETE FROM Category WHERE Id IN ({0});
                ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(BatchSize));

                    skip += BatchSize;
                }
                while (skip < ids.Length);
            }
        }

        public async Task RemoveItemsAsync(CatalogDbContext dbContext, string[] itemIds)
        {
            if (!itemIds.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                        DELETE SEO FROM CatalogSeoInfo SEO INNER JOIN Item I ON I.Id = SEO.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE CR FROM CategoryItemRelation  CR INNER JOIN Item I ON I.Id = CR.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE CI FROM CatalogImage CI INNER JOIN Item I ON I.Id = CI.ItemId
                        WHERE I.Id IN ({0})  OR I.ParentId IN ({0});

                        DELETE CA FROM CatalogAsset CA INNER JOIN Item I ON I.Id = CA.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE PV FROM PropertyValue PV INNER JOIN Item I ON I.Id = PV.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE ER FROM EditorialReview ER INNER JOIN Item I ON I.Id = ER.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE A FROM Association A INNER JOIN Item I ON I.Id = A.ItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE A FROM Association A INNER JOIN Item I ON I.Id = A.AssociatedItemId
                        WHERE I.Id IN ({0}) OR I.ParentId IN ({0});

                        DELETE  FROM Item  WHERE ParentId IN ({0});

                        DELETE  FROM Item  WHERE Id IN ({0});
                    ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, itemIds.Skip(skip).Take(BatchSize));

                    skip += BatchSize;
                } while (skip < itemIds.Length);

            }
        }

        protected virtual async Task<int> ExecuteStoreQueryAsync(CatalogDbContext dbContext, string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return await dbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters.ToArray());
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new MySqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToList(),
            };
        }

        protected class Command
        {
            public string Text { get; set; } = string.Empty;
            public IList<object> Parameters { get; set; } = new List<object>();
        }

    }
}
