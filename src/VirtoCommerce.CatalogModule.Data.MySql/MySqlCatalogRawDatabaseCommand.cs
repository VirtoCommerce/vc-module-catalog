using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Extensions;

namespace VirtoCommerce.CatalogModule.Data.MySql
{
    public class MySqlCatalogRawDatabaseCommand : ICatalogRawDatabaseCommand
    {
        private const int BatchSize = 500;

        public async Task<IList<string>> GetAllChildrenCategoriesIdsAsync(CatalogDbContext dbContext, IList<string> categoryIds)
        {
            if (categoryIds.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new HashSet<string>();

            var nextLevelCategories = new HashSet<string>(categoryIds);

            var allChildCategories = await dbContext.Set<CategoryEntity>()
                .Where(item => item.ParentCategoryId != null)
                .Select(item => new { item.Id, item.ParentCategoryId })
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

        public async Task<IList<string>> GetAllSeoDuplicatesIdsAsync(CatalogDbContext dbContext)
        {
            const string commandTemplate = @"
                        SELECT cs.Id from CatalogSeoInfo as cs where cs.Id not in (
                        SELECT b.Id from (SELECT MIN(c.Id)  as Id FROM CatalogSeoInfo as c GROUP BY c.Keyword, c.StoreId) b)
                ";

            var command = CreateCommand(commandTemplate, Array.Empty<string>());
            var result = await dbContext.ExecuteArrayAsync<string>(command.Text, command.Parameters.ToArray());

            return result ?? Array.Empty<string>();
        }

        protected virtual async Task<List<AssociationEntity>> GetCategoriesAssociationsAsync(CatalogDbContext dbContext, IReadOnlyDictionary<string, string> associationsToCategories)
        {
            if (associationsToCategories.IsNullOrEmpty())
            {
                return new List<AssociationEntity>();
            }

            var result = new List<AssociationEntity>();

            var commandTemplate =
                @"SELECT CONVERT( UUID(), char(128) ) AS Id,
                         AssociationType, Priority, Quantity, Tags, OuterId, a.ItemId AS ItemId,
                         itemIds.ItemId AS AssociatedItemId,
                         AssociatedCategoryId,
                         CreatedDate,
                         ModifiedDate,
                         CreatedBy,
                         ModifiedBy  FROM (
                                      SELECT AssociationId, ItemId FROM (
                                              SELECT t2.Id CategoryId, @associationId AssociationId  FROM(
                                              SELECT @r AS _id,
                                              (SELECT @r:= ParentCategoryId FROM Category WHERE Id = _id) AS _parent_id,
                                                         @l:= @l + 1 AS level
                                               FROM(SELECT @r:= @categoryId, @l:= 0) val, Category
                                               WHERE @r IS NOT NULL) t1
                                               JOIN Category t2 ON t1._id = t2.Id) cat JOIN (
                 SELECT CategoryId, Id AS ItemId FROM Item WHERE ParentId IS NULL) items ON cat.CategoryId = items.CategoryId) itemIds
                 JOIN( SELECT* FROM Association WHERE Id = @associationId) a
                 ON itemIds.AssociationId = a.Id";

            // I haven't found a way to get all associations in one query. As such do it in a cycle for each association to category
            foreach (var pair in associationsToCategories)
            {
                var associationIdParam = new MySqlParameter("@associationId", pair.Key);
                var categoryIdParam = new MySqlParameter("@categoryId", pair.Value);
                var items = await dbContext.Set<AssociationEntity>().FromSqlRaw(commandTemplate, associationIdParam, categoryIdParam).ToListAsync();

                result.AddRange(items);
            }

            return result;
        }

        private class AssociationEntityComparer : IEqualityComparer<AssociationEntity>
        {
            public bool Equals(AssociationEntity x, AssociationEntity y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.ItemId == y.ItemId && x.AssociatedItemId == y.AssociatedItemId;
            }

            public int GetHashCode(AssociationEntity obj)
            {
                return HashCode.Combine(obj.ItemId, obj.AssociatedItemId);
            }
        }

        private static readonly AssociationEntityComparer AssociationComparer = new();

        public async Task<GenericSearchResult<AssociationEntity>> SearchAssociations(CatalogDbContext dbContext, ProductAssociationSearchCriteria criteria)
        {
            var result = new GenericSearchResult<AssociationEntity>();

            var query = dbContext.Set<AssociationEntity>().AsQueryable();

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(item => criteria.ObjectIds.Contains(item.ItemId));
            }

            if (!criteria.AssociatedObjectIds.IsNullOrEmpty())
            {
                query = query.Where(item => criteria.AssociatedObjectIds.Contains(item.AssociatedItemId));
            }

            if (!string.IsNullOrEmpty(criteria.Group))
            {
                query = query.Where(item => item.AssociationType == criteria.Group);
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(item => EF.Functions.Like(item.Item.Name, $"%{criteria.Keyword}%"));
            }

            // There's no built in string_split function in MySql so we have to materialize the collection to perform the search by tags
            var resultList = await query.ToListAsync();

            // Search by tags if any
            if (!criteria.Tags.IsNullOrEmpty())
            {
                var tags = new HashSet<string>(criteria.Tags);

                resultList.RemoveAll(item => item.Tags.IsNullOrEmpty() || !tags.Intersect(item.Tags.Split(';')).Any());
            }

            // Substitute associations to category with related items associations
            var categoryAssociations = resultList.Where(item => item.AssociatedCategoryId != null).ToList();

            foreach (var a in categoryAssociations)
            {
                resultList.Remove(a);
            }

            var itemsFromCategories = await GetCategoriesAssociationsAsync(dbContext,
                categoryAssociations.ToDictionary(item => item.Id, item => item.AssociatedCategoryId));

            resultList.AddRange(itemsFromCategories);

            // Just in case remove all associations without associated item
            resultList.RemoveAll(item => item.AssociatedItemId == null);

            // Remove duplicate associations if any
            var finalResultList = resultList.Distinct(AssociationComparer).ToList();

            // The ordering is required to get the same result list in case of multiple requests with the same search criteria
            result.TotalCount = finalResultList.Count;
            result.Results = criteria.Take > 0
                ? finalResultList.OrderBy(item => item.Priority)
                            .ThenBy(item => item.ItemId)
                            .ThenBy(item => item.AssociatedItemId)
                            .Skip(criteria.Skip)
                            .Take(criteria.Take)
                            .ToList()
                : new List<AssociationEntity>();

            return result;
        }

        public async Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(CatalogDbContext dbContext, string categoryId)
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

        public async Task RemoveCatalogsAsync(CatalogDbContext dbContext, IList<string> ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                    DELETE FROM CatalogLanguage WHERE CatalogId IN ({0});
                    DELETE FROM CategoryRelation WHERE TargetCatalogId IN ({0});
                    DELETE FROM CategoryItemRelation WHERE CatalogId IN ({0});
                    DELETE FROM PropertyValue WHERE CatalogId IN ({0});
                    DELETE FROM Property WHERE CatalogId IN ({0});
                    DELETE FROM Catalog WHERE Id IN ({0});
                    ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(BatchSize));
                    skip += BatchSize;
                }
                while (skip < ids.Count);
            }
        }

        public async Task RemoveCategoriesAsync(CatalogDbContext dbContext, IList<string> ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var skip = 0;
                do
                {
                    const string commandTemplate = @"
                    DELETE FROM CatalogSeoInfo WHERE CategoryId IN ({0});
                    DELETE FROM CatalogImage WHERE CategoryId IN ({0});
                    DELETE FROM PropertyValue WHERE CategoryId IN ({0});
                    DELETE CR FROM CategoryRelation CR INNER JOIN Category C ON C.Id = CR.SourceCategoryId OR C.Id = CR.TargetCategoryId WHERE C.Id IN ({0});
                    DELETE FROM CategoryItemRelation WHERE CategoryId IN ({0});
                    DELETE FROM Association WHERE AssociatedCategoryId IN ({0});
                    DELETE FROM Property WHERE CategoryId IN ({0});
                    DELETE FROM CategoryDescription WHERE CategoryId IN ({0});
                    DELETE FROM Category WHERE Id IN ({0});
                    ";

                    await ExecuteStoreQueryAsync(dbContext, commandTemplate, ids.Skip(skip).Take(BatchSize));

                    skip += BatchSize;
                }
                while (skip < ids.Count);
            }
        }

        public async Task RemoveItemsAsync(CatalogDbContext dbContext, IList<string> itemIds)
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
                } while (skip < itemIds.Count);
            }
        }

        protected virtual async Task<int> ExecuteStoreQueryAsync(CatalogDbContext dbContext, string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return await dbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters.ToArray());
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            if (!parameterValues.IsNullOrEmpty())
            {
                var parameters = parameterValues.Select((v, i) => new MySqlParameter($"@p{i}", v)).ToArray();
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

        protected class Command
        {
            public string Text { get; set; } = string.Empty;
            public IList<object> Parameters { get; set; } = new List<object>();
        }
    }
}
