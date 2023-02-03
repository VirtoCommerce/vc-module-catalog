using System.Text;

namespace VirtoCommerce.CatalogModule.Data.PostgreSql
{
    public class PostgreSqlCatalogRawDatabaseCommand : ICatalogRawDatabaseCommand
    {
        private const int batchSize = 500;

        public PostgreSqlCatalogRawDatabaseCommand()
        {
        }

        public virtual async Task<string[]> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, string[] categoryIds)
        {
            var result = Array.Empty<string>();

            if (!categoryIds.IsNullOrEmpty())
            {
                const string commandTemplate = @"
                WITH RECURSIVE cte AS (
                    SELECT a.""Id"" FROM ""Category"" a WHERE ""Id"" IN ({0})
                UNION ALL
                    SELECT a.""Id"" FROM ""Category"" a
                        JOIN cte c
                            ON a.""ParentCategoryId"" = c.""Id""
                )
                SELECT ""Id"" FROM cte WHERE ""Id"" NOT IN ({0})
                ";

                var getAllChildrenCategoriesCommand = CreateCommand(commandTemplate, categoryIds);
                result = await dbContext.ExecuteArrayAsync<string>(getAllChildrenCategoriesCommand.Text, getAllChildrenCategoriesCommand.Parameters.ToArray());
            }

            return result;
        }

        public virtual async Task<string[]> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext)
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

        public virtual async Task RemoveCatalogsAsync(CatalogDbContext dbContext, string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                        DELETE FROM ""CatalogLanguage"" CL USING ""Catalog"" C WHERE C.""Id"" = CL.""CatalogId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""CategoryRelation"" CR USING ""Catalog"" C WHERE C.""Id"" = CR.""TargetCatalogId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""PropertyValue"" PV USING ""Catalog"" C WHERE C.""Id"" = PV.""CatalogId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""Property"" P USING ""Catalog"" C WHERE C.""Id"" = P.""CatalogId""  AND C.""Id"" IN ({0});
                        DELETE FROM ""Catalog"" WHERE ""Id"" IN ({0});
                    ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(batchSize));
                    skip += batchSize;
                }
                while (skip < ids.Length);
            }
        }

        public virtual async Task RemoveCategoriesAsync(CatalogDbContext dbContext, string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                        DELETE FROM ""CatalogSeoInfo"" SEO USING ""Category"" C WHERE C.""Id"" = SEO.""CategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""CatalogImage"" CI USING ""Category"" C WHERE C.""Id"" = CI.""CategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""PropertyValue"" PV USING ""Category"" C WHERE C.""Id"" = PV.""CategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""CategoryRelation"" CR USING ""Category"" C WHERE (C.""Id"" = CR.""SourceCategoryId"" OR C.""Id"" = CR.""TargetCategoryId"") AND C.""Id"" IN ({0});
                        DELETE FROM ""CategoryItemRelation"" CIR USING ""Category"" C WHERE C.""Id"" = CIR.""CategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""Association"" A USING ""Category"" C WHERE C.""Id"" = A.""AssociatedCategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""Property"" P USING ""Category"" C WHERE C.""Id"" = P.""CategoryId""  AND C.""Id"" IN ({0});
                        DELETE FROM ""CategoryDescription"" D USING ""Category"" C WHERE C.""Id"" = D.""CategoryId"" AND C.""Id"" IN ({0});
                        DELETE FROM ""Category"" WHERE ""Id"" IN ({0});
                ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(batchSize));

                    skip += batchSize;
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

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, itemIds.Skip(skip).Take(batchSize));

                    skip += batchSize;
                }
                while (skip < itemIds.Length);
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
                commands.ForEach(x => x.Parameters.Add(new NpgsqlParameter($"@group", criteria.Group)));
            }

            if (!criteria.Tags.IsNullOrEmpty())
            {
                commands.ForEach(x => AddArrayParameters(x, "@tags", criteria.Tags));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var wildcardKeyword = $"%{criteria.Keyword}%";
                commands.ForEach(x => x.Parameters.Add(new NpgsqlParameter($"@keyword", wildcardKeyword)));
            }

            if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
            {
                commands.ForEach(x => AddArrayParameters(x, "@associatedObjectIds", criteria.AssociatedObjectIds));
            }

            result.TotalCount = await dbContext.ExecuteScalarAsync<int>(countSqlCommand.Text, countSqlCommand.Parameters.ToArray());
            result.Results = criteria.Take > 0
                ? await dbContext.Set<AssociationEntity>().FromSqlRaw(querySqlCommand.Text, querySqlCommand.Parameters.ToArray()).ToListAsync()
                : new List<AssociationEntity>();

            return result;
        }

        public virtual async Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId)
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
                ; WITH RECURSIVE Association_CTE AS
                (
                    SELECT a.*
                    FROM ""Association"" a");

            AddAssociationsSearchCriteraToCommand(command, criteria);

            command.Append(@"), Category_CTE AS
                (
                    SELECT ""AssociatedCategoryId"" Id
                    FROM Association_CTE
                    WHERE ""AssociatedCategoryId"" IS NOT NULL
                    UNION ALL
                    SELECT c.""Id""
                    FROM ""Category"" c
                    INNER JOIN Category_CTE cte ON c.""ParentCategoryId"" = cte.Id
                ),
                Item_CTE AS
                (
                    SELECT  i.""Id""
                    FROM (SELECT DISTINCT Id FROM Category_CTE) c
                    LEFT JOIN ""Item"" i ON c.Id=i.""CategoryId"" WHERE i.""ParentId"" IS NULL
                    UNION
                    SELECT ""AssociatedItemId"" Id FROM Association_CTE
                )
                SELECT COUNT(""Id"") FROM Item_CTE");

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

            AddAssociationsSearchCriteraToCommand(command, criteria);

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
                            uuid_generate_v4()::text as Id
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

        protected virtual void AddAssociationsSearchCriteraToCommand(StringBuilder command, ProductAssociationSearchCriteria criteria)
        {
            // join items to search by keyword
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                command.Append(@"
                    left join ""Item"" i on i.""Id"" = a.""AssociatedItemId""
                ");
            }

            command.Append(@"
                    WHERE ""ItemId"" IN ({0})
            ");

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

            // search by associated product ids
            if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
            {
                command.Append("  AND a.\"AssociatedItemId\" in (@associatedObjectIds)");
            }
        }

        protected virtual Task<int> ExecuteStoreQueryAsync(CatalogDbContext dbContext, string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return dbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters.ToArray());
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new NpgsqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToList(),
            };
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
