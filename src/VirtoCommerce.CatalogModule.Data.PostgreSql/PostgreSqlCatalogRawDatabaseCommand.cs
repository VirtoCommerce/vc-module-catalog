using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Extensions;

namespace VirtoCommerce.CatalogModule.Data.PostgreSql
{
    public class PostgreSqlCatalogRawDatabaseCommand : ICatalogRawDatabaseCommand
    {
        private const int batchSize = 500;

        public async Task<IList<string>> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, IList<string> categoryIds)
        {
            var result = await GetChildCategoriesAsync(dbContext, categoryIds);

            return result.Select(x => x.Id).ToList();
        }

        public virtual async Task<IList<CategoryHierarchyItem>> GetChildCategoriesAsync(CatalogDbContext dbContext, IList<string> categoryIds)
        {
            if (categoryIds.IsNullOrEmpty())
            {
                return Array.Empty<CategoryHierarchyItem>();
            }

            var result = new List<CategoryHierarchyItem>();

            const string commandTemplate = @"
            WITH RECURSIVE CategoryHierarchy AS (
                SELECT a.""Id"", a.""ParentCategoryId"", 0 AS ""Depth"" FROM ""Category"" a
                WHERE a.""Id"" IN ({0})
                UNION ALL
                SELECT c.""Id"", c.""ParentCategoryId"", (ch.""Depth"" + 1) AS ""Depth"" FROM ""Category"" c
                INNER JOIN CategoryHierarchy ch ON c.""ParentCategoryId"" = ch.""Id""
            )
            SELECT ""Id"", ""ParentCategoryId"", ""Depth"" FROM CategoryHierarchy WHERE ""Id"" NOT IN ({0});
            ";

            foreach (var categoryIdsPage in categoryIds.Paginate(batchSize))
            {
                var getAllChildrenCategoriesCommand = CreateCommand(commandTemplate, categoryIdsPage);
                var batchResult = await dbContext.ExecuteEntityArrayAsync<CategoryHierarchyItem>(getAllChildrenCategoriesCommand.Text, getAllChildrenCategoriesCommand.Parameters.ToArray());
                if (!batchResult.IsNullOrEmpty())
                {
                    result.AddRange(batchResult);
                }
            }

            return result;
        }

        public virtual async Task<IList<string>> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext)
        {

            const string commandTemplate = @"
                    WITH cte AS (
                        SELECT
                            ""Id"",
                            ""Keyword"",
                            ""StoreId"",
                            ROW_NUMBER() OVER ( PARTITION BY ""Keyword"", ""StoreId"" ORDER BY ""StoreId"") ""row_num""
                        FROM ""CatalogSeoInfo""
                    )
                    SELECT ""Id"" FROM cte
                    WHERE ""row_num"" > 1
                ";

            var command = CreateCommand(commandTemplate, Array.Empty<string>());
            var result = await dbContext.ExecuteArrayAsync<string>(command.Text, command.Parameters.ToArray());

            return result ?? Array.Empty<string>();
        }

        public virtual async Task RemoveAllPropertyValuesAsync(CatalogDbContext dbContext, PropertyEntity catalogProperty, PropertyEntity categoryProperty, PropertyEntity itemProperty)
        {
            string commandText;
            if (catalogProperty != null)
            {
                commandText = $@"DELETE FROM ""PropertyValue"" PV USING ""Catalog"" C WHERE C.""Id"" = PV.""CatalogId"" AND C.""Id"" = '{catalogProperty.CatalogId}' AND PV.""Name"" = '{catalogProperty.Name}';";
                await dbContext.Database.ExecuteSqlRawAsync(commandText);
            }
            if (categoryProperty != null)
            {
                commandText = $@"DELETE FROM ""PropertyValue"" PV USING ""Category"" C WHERE C.""Id"" = PV.""CategoryId"" AND C.""CatalogId"" = '{categoryProperty.CatalogId}' AND PV.""Name"" = '{categoryProperty.Name}';";
                await dbContext.Database.ExecuteSqlRawAsync(commandText);
            }
            if (itemProperty != null)
            {
                commandText = $@"DELETE FROM ""PropertyValue"" PV USING ""Item"" I WHERE I.""Id"" = PV.""ItemId"" AND I.""CatalogId"" = '{itemProperty.CatalogId}' AND PV.""Name"" = '{itemProperty.Name}';";
                await dbContext.Database.ExecuteSqlRawAsync(commandText);
            }
        }

        public virtual async Task RemoveCatalogsAsync(CatalogDbContext dbContext, IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            const string commandTemplate = @"
                        DELETE FROM ""CatalogLanguage"" WHERE ""CatalogId"" IN ({0});
                        DELETE FROM ""CategoryRelation""WHERE ""TargetCatalogId"" IN ({0});
                        DELETE FROM ""CategoryItemRelation"" WHERE ""CatalogId"" IN ({0});
                        DELETE FROM ""PropertyValue"" WHERE ""CatalogId"" IN ({0});
                        DELETE FROM ""Property"" WHERE ""CatalogId"" IN ({0});
                        DELETE FROM ""Catalog"" WHERE ""Id"" IN ({0});
                        ";

            foreach (var idsPage in ids.Paginate(batchSize))
            {
                await ExecuteStoreQueryAsync(dbContext, commandTemplate, idsPage);
            }
        }

        public virtual async Task RemoveCategoriesAsync(CatalogDbContext dbContext, IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            const string commandTemplate = @"
                DELETE FROM ""CatalogSeoInfo"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""CatalogImage"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""PropertyValue"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""CategoryRelation"" CR USING ""Category"" C WHERE (C.""Id"" = CR.""SourceCategoryId"" OR C.""Id"" = CR.""TargetCategoryId"") AND C.""Id"" IN ({0});
                DELETE FROM ""CategoryItemRelation"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""Association"" WHERE ""AssociatedCategoryId"" IN ({0});
                DELETE FROM ""Property"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""CategoryDescription"" WHERE ""CategoryId"" IN ({0});
                DELETE FROM ""Category"" WHERE ""Id"" IN ({0});
                        ";

            foreach (var idsPage in ids.Paginate(batchSize))
            {
                await ExecuteStoreQueryAsync(dbContext, commandTemplate, idsPage);
            }
        }

        public async Task RemoveItemsAsync(CatalogDbContext dbContext, IList<string> itemIds)
        {
            if (itemIds.IsNullOrEmpty())
            {
                return;
            }
            {
                const string commandTemplate = @"
                        DELETE FROM ""CatalogSeoInfo"" SEO USING ""Item"" I WHERE I.""Id"" = SEO.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""CategoryItemRelation"" CR USING ""Item"" I WHERE I.""Id"" = CR.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""CatalogImage"" CI USING ""Item"" I WHERE I.""Id"" = CI.""ItemId""
                        AND I.""Id"" IN ({0})  OR I.""ParentId"" IN ({0});

                        DELETE FROM ""CatalogAsset"" CA USING ""Item"" I WHERE I.""Id"" = CA.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""PropertyValue"" PV USING ""Item"" I WHERE I.""Id"" = PV.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""EditorialReview"" ER USING ""Item"" I WHERE I.""Id"" = ER.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""Association"" A USING ""Item"" I WHERE I.""Id"" = A.""ItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""Association"" A USING ""Item"" I WHERE I.""Id"" = A.""AssociatedItemId""
                        AND I.""Id"" IN ({0}) OR I.""ParentId"" IN ({0});

                        DELETE FROM ""Item"" WHERE ""ParentId"" IN ({0});

                        DELETE FROM ""Item"" WHERE ""Id"" IN ({0});
                    ";

                foreach (var itemIdsPage in itemIds.Paginate(batchSize))
                {
                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, itemIdsPage);
                }
            }
        }

        public virtual async Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria)
        {
            var result = new GenericSearchResult<AssociationEntity>();

            var countSqlCommand = CreateCommand(GetAssociationsCountSqlCommandText(criteria), criteria.ObjectIds);
            var querySqlCommand = CreateCommand(GetAssociationsQuerySqlCommandText(criteria), criteria.ObjectIds);

            var commands = new List<Command> { countSqlCommand };

            if (criteria.Take > 0)
            {
                commands.Add(querySqlCommand);
            }

            if (!string.IsNullOrEmpty(criteria.Group))
            {
                commands.ForEach(x => x.Parameters.Add(new NpgsqlParameter("@group", criteria.Group)));
            }

            if (!criteria.Tags.IsNullOrEmpty())
            {
                commands.ForEach(x => AddArrayParameters(x, "@tags", criteria.Tags));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var wildcardKeyword = $"%{criteria.Keyword}%";
                commands.ForEach(x => x.Parameters.Add(new NpgsqlParameter("@keyword", wildcardKeyword)));
            }

            if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
            {
                commands.ForEach(x => AddArrayParameters(x, "@associatedObjectIds", criteria.AssociatedObjectIds));
            }

            var lTotalCount = await dbContext.ExecuteScalarAsync<long>(countSqlCommand.Text, countSqlCommand.Parameters.ToArray());
            result.TotalCount = (int)lTotalCount;
            result.Results = criteria.Take > 0
                ? await dbContext.Set<AssociationEntity>().FromSqlRaw(querySqlCommand.Text, querySqlCommand.Parameters.ToArray()).ToListAsync()
                : new List<AssociationEntity>();

            return result;
        }

        public virtual async Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId)
        {
            var commandTemplate = @"
            WITH RECURSIVE cp AS (
                SELECT * FROM ""Category"" WHERE ""Id"" = @categoryId
            UNION ALL
                SELECT c.* FROM ""Category"" AS c
                    JOIN cp
                        ON c.""Id"" = cp.""ParentCategoryId""
            )
            SELECT * FROM cp
            ";

            var categoryIdParam = new NpgsqlParameter("@categoryId", categoryId);
            var result = await dbContext.Set<CategoryEntity>().FromSqlRaw(commandTemplate, categoryIdParam).ToListAsync();

            return result;
        }

        protected virtual string GetAssociationsCountSqlCommandText(ProductAssociationSearchCriteria criteria)
        {
            var command = new StringBuilder();

            command.Append(@"
                    ;WITH RECURSIVE Association_CTE AS
                    (
                        SELECT
                             a.""Id""
                            ,a.""AssociationType""
                            ,a.""Priority""
                            ,a.""ItemId""
                            ,a.""CreatedDate""
                            ,a.""ModifiedDate""
                            ,a.""CreatedBy""
                            ,a.""ModifiedBy""
                            ,a.""AssociatedItemId""
                            ,a.""AssociatedCategoryId""
                            ,a.""Tags""
                            ,a.""Quantity""
                            ,a.""OuterId""
                        FROM ""Association"" a"
            );

            AddAssociationsSearchCriteriaToCommand(command, criteria);

            command.Append(@"), Category_CTE AS
                    (
                        SELECT ""AssociatedCategoryId"" Id, ""AssociatedCategoryId""
                        FROM Association_CTE
                        WHERE ""AssociatedCategoryId"" IS NOT NULL
                        UNION ALL
                        SELECT c.""Id"", cte.""AssociatedCategoryId""
                        FROM ""Category"" c
                        INNER JOIN Category_CTE cte ON c.""ParentCategoryId"" = cte.Id
                    ),
                    Item_CTE AS
                    (
                        SELECT
                            a.""Id""
                            ,a.""AssociationType""
                            ,a.""Priority""
                            ,a.""ItemId""
                            ,a.""CreatedDate""
                            ,a.""ModifiedDate""
                            ,a.""CreatedBy""
                            ,a.""ModifiedBy""
                            ,i.""Id"" ""AssociatedItemId""
                            ,a.""AssociatedCategoryId""
                            ,a.""Tags""
                            ,a.""Quantity""
                            ,a.""OuterId""
                        FROM Category_CTE cat
                        LEFT JOIN ""Item"" i ON cat.Id=i.""CategoryId""
                        LEFT JOIN ""Association"" a ON cat.""AssociatedCategoryId""=a.""AssociatedCategoryId""
                        WHERE i.""ParentId"" IS NULL
                        UNION
                        SELECT * FROM Association_CTE
                    )
                    SELECT COUNT(*) FROM Item_CTE WHERE ""AssociatedItemId"" IS NOT NULL ");

            return command.ToString();
        }

        protected virtual string GetAssociationsQuerySqlCommandText(ProductAssociationSearchCriteria criteria)
        {
            var command = new StringBuilder();

            command.Append(@"
                    ;WITH RECURSIVE Association_CTE AS
                    (
                        SELECT
                             a.""Id""
                            ,a.""AssociationType""
                            ,a.""Priority""
                            ,a.""ItemId""
                            ,a.""CreatedDate""
                            ,a.""ModifiedDate""
                            ,a.""CreatedBy""
                            ,a.""ModifiedBy""
                            ,a.""AssociatedItemId""
                            ,a.""AssociatedCategoryId""
                            ,a.""Tags""
                            ,a.""Quantity""
                            ,a.""OuterId""
                        FROM ""Association"" a"
            );

            AddAssociationsSearchCriteriaToCommand(command, criteria);

            command.Append(@"), Category_CTE AS
                    (
                        SELECT ""AssociatedCategoryId"" Id, ""AssociatedCategoryId""
                        FROM Association_CTE
                        WHERE ""AssociatedCategoryId"" IS NOT NULL
                        UNION ALL
                        SELECT c.""Id"", cte.""AssociatedCategoryId""
                        FROM ""Category"" c
                        INNER JOIN Category_CTE cte ON c.""ParentCategoryId"" = cte.Id
                    ),
                    Item_CTE AS
                    (
                        SELECT
                            a.""Id""
                            ,a.""AssociationType""
                            ,a.""Priority""
                            ,a.""ItemId""
                            ,a.""CreatedDate""
                            ,a.""ModifiedDate""
                            ,a.""CreatedBy""
                            ,a.""ModifiedBy""
                            ,i.""Id"" ""AssociatedItemId""
                            ,a.""AssociatedCategoryId""
                            ,a.""Tags""
                            ,a.""Quantity""
                            ,a.""OuterId""
                        FROM Category_CTE cat
                        LEFT JOIN ""Item"" i ON cat.Id=i.""CategoryId""
                        LEFT JOIN ""Association"" a ON cat.""AssociatedCategoryId""=a.""AssociatedCategoryId""
                        WHERE i.""ParentId"" IS NULL
                        UNION
                        SELECT * FROM Association_CTE
                    )
                    SELECT * FROM Item_CTE WHERE ""AssociatedItemId"" IS NOT NULL ORDER BY ""Priority"" ");

            command.Append($"OFFSET {criteria.Skip} ROWS FETCH NEXT {criteria.Take} ROWS ONLY");

            return command.ToString();
        }

        protected virtual void AddAssociationsSearchCriteriaToCommand(StringBuilder command, ProductAssociationSearchCriteria criteria)
        {
            // join items to search by keyword
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                command.Append(@"
                    left join ""Item"" i on i.""Id"" = a.""AssociatedItemId""
                ");
            }

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                command.Append(@"
                    WHERE ""ItemId"" IN ({0})
                ");

                // search by associated product ids
                if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
                {
                    command.Append("  AND a.\"AssociatedItemId\" in (@associatedObjectIds)");
                }
            }
            else
            {
                // search by associated product ids
                if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
                {
                    command.Append("  WHERE a.\"AssociatedItemId\" in (@associatedObjectIds)");
                }
            }

            // search by association type
            if (!string.IsNullOrEmpty(criteria.Group))
            {
                command.Append("  AND \"AssociationType\" = @group");
            }

            // search by association tags
            if (!criteria.Tags.IsNullOrEmpty())
            {
                command.Append("  AND exists( SELECT value FROM string_to_array(\"Tags\", ';') WHERE value IN (@tags))");
            }

            // search by keyword
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                command.Append("  AND i.\"Name\" like @keyword");
            }
        }

        protected virtual Task<int> ExecuteStoreQueryAsync(CatalogDbContext dbContext, string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return dbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters.ToArray());
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            if (!parameterValues.IsNullOrEmpty())
            {
                var parameters = parameterValues.Select((v, i) => new NpgsqlParameter($"@p{i}", v)).ToArray();
                var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

                return new Command
                {
                    Text = string.Format(commandTemplate, parameterNames),
                    Parameters = parameters.OfType<object>().ToList(),
                };
            }
            else
            {
                return new Command
                {
                    Text = commandTemplate
                };
            }
        }

        protected static NpgsqlParameter[] AddArrayParameters<T>(Command cmd, string paramNameRoot, IEnumerable<T> values)
        {
            /* An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually.
             * Each item in the array will end up being it's own NpgsqlParameter so the return value for this must be used as part of the
             * IN statement in the CommandText.
             */
            var parameters = new List<NpgsqlParameter>();
            var parameterNames = new List<string>();
            var paramNbr = 1;
            foreach (var value in values)
            {
                var paramName = $"{paramNameRoot}{paramNbr++}";
                parameterNames.Add(paramName);
                var p = new NpgsqlParameter(paramName, value);
                cmd.Parameters.Add(p);
                parameters.Add(p);
            }
            cmd.Text = cmd.Text.Replace(paramNameRoot, string.Join(",", parameterNames));

            return parameters.ToArray();
        }

        protected class Command
        {
            public string Text { get; set; } = string.Empty;
            public IList<object> Parameters { get; set; } = new List<object>();
        }
    }
}
