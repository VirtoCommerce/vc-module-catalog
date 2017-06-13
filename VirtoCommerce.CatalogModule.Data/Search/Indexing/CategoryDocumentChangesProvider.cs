using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class CategoryDocumentChangesProvider : IIndexDocumentChangesProvider
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;

        public CategoryDocumentChangesProvider(Func<ICatalogRepository> catalogRepositoryFactory)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
        }

        public virtual Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            using (var repository = _catalogRepositoryFactory())
            {
                result = GetCategoryQuery(repository, startDate, endDate).Count();
            }

            return Task.FromResult(result);
        }

        public virtual Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            using (var repository = _catalogRepositoryFactory())
            {
                var query = GetCategoryQuery(repository, startDate, endDate);

                if (startDate == null && endDate == null)
                {
                    query = query.OrderBy(c => c.CreatedDate);
                }
                else
                {
                    query = query.OrderBy(c => c.ModifiedDate);
                }

                var categories = query.Skip((int)skip).Take((int)take).ToArray();

                result = categories.Select(c =>
                    new IndexDocumentChange
                    {
                        DocumentId = c.Id,
                        ChangeType = IndexDocumentChangeType.Modified,
                        ChangeDate = c.ModifiedDate ?? c.CreatedDate,
                    }
                ).ToArray();
            }

            return Task.FromResult(result);
        }


        private static IQueryable<Category> GetCategoryQuery(ICatalogRepository repository, DateTime? startDate, DateTime? endDate)
        {
            var query = repository.Categories;

            if (startDate != null)
            {
                query = query.Where(c => c.ModifiedDate > startDate);
            }

            if (endDate != null)
            {
                query = query.Where(c => c.ModifiedDate <= endDate);
            }

            return query;
        }
    }
}
