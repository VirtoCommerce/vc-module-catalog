using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Data.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportPagedDataSource : ComplexExportPagedDataSource<CatalogFullExportDataQuery>
    {
        private readonly ICatalogService _catalogService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IPropertyService _propertyService;
        private readonly IAssociationService _associationService;
        private readonly IProperyDictionaryItemSearchService _propertyDictionarySearchService;
        private readonly IProperyDictionaryItemService _propertyDictionaryService;

        public CatalogFullExportPagedDataSource(ICatalogSearchService catalogSearchService,
            ICatalogService catalogService,
            ICategoryService categoryService,
            IItemService itemService,
            IPropertyService propertyService,
            IAssociationService associationService,
            IProperyDictionaryItemSearchService propertyDictionarySearchService,
            IProperyDictionaryItemService propertyDictionaryService,
            CatalogFullExportDataQuery dataQuery)
        : base(dataQuery)
        {
            _catalogSearchService = catalogSearchService;
            _catalogService = catalogService;
            _categoryService = categoryService;
            _itemService = itemService;
            _propertyService = propertyService;
            _associationService = associationService;
            _propertyDictionarySearchService = propertyDictionarySearchService;
            _propertyDictionaryService = propertyDictionaryService;
        }

        protected override void InitDataSourceStates()
        {
            _exportDataSourceStates.AddRange(new ExportDataSourceState[]
            {
                // -- ExportProperties
                new ExportDataSourceState
                {
                    FetchFunc = (x) => Task.Factory.StartNew( ()=>
                    {
                        var allproperties = _propertyService.GetAllProperties(); // (!!!!- It needs to filter by CatalogIDs
                        var properties = allproperties.Skip(x.Skip).Take(x.Take); // (!!!!- Terrible, but _propertyService does not have paged read)
                        //Load property dictionary values and reset some props to decrease size of the resulting json 
                        foreach (var property in properties)
                        {
                            ResetRedundantReferences(property);
                        }
                        x.TotalCount = allproperties.Count();
                        x.Result = properties.Cast<ExportableProperty>();
                    }
                    )
                },

                //ExportPropertiesDictionaryItems
                new ExportDataSourceState
                {
                    FetchFunc = (x) => Task.Factory.StartNew( ()=>
                    {
                        var criteria = new PropertyDictionaryItemSearchCriteria { Take = x.Take, Skip = x.Skip }; // (!!!!- It needs to filter by CatalogIDs
                        var searchResponse = _propertyDictionarySearchService.Search(criteria);
                        x.TotalCount = searchResponse.TotalCount;
                        x.Result = searchResponse.Results.Cast<ExportablePropertyDictionaryItem>();
                    }
                    )
                },
                // ExportCatalogs
                new ExportDataSourceState
                {
                    FetchFunc = (x) => Task.Factory.StartNew( ()=>
                    {

                        var allcatalogs = _catalogService.GetCatalogsList().ToArray(); // (!!!!- It needs to filter by CatalogIDs
                        var catalogs = allcatalogs.Skip(x.Skip).Take(x.Take); // (!!!!- Terrible, but _catalogService does not have paged read)
                        foreach (var catalog in catalogs)
                        {
                            ResetRedundantReferences(catalog);
                        }
                        x.TotalCount = allcatalogs.Count();
                        x.Result = catalogs.Cast<ExportableCatalog>();
                    }
                    )
                },
                // ExportCategories
                new ExportDataSourceState
                {
                    FetchFunc = (x) => Task.Factory.StartNew( ()=>
                    {
                        var categorySearchCriteria = new SearchCriteria { WithHidden = true, Skip = x.Skip, Take = x.Take, ResponseGroup = SearchResponseGroup.WithCategories }; // (!!!!- It needs to filter by CatalogIDs
                        var categoriesSearchResult = _catalogSearchService.Search(categorySearchCriteria);
                        var categories = _categoryService.GetByIds(categoriesSearchResult.Categories.Select(y => y.Id).ToArray(), CategoryResponseGroup.Full);

                        //reset some properties to decrease resulting JSON size
                        foreach (var category in categories)
                        {
                            ResetRedundantReferences(category);
                        }

                        x.TotalCount = categoriesSearchResult.Categories.Count; // (!!!!- No hope about total no of categories
                        x.Result = categories.Cast<ExportableCategory>();
                    }
                    )
                },
                // ExportProducts
                new ExportDataSourceState
                {
                    FetchFunc = (x) => Task.Factory.StartNew( ()=>
                    {
                        var productSearchCriteria = new SearchCriteria { WithHidden = true, Take = x.Take, Skip = x.Skip, ResponseGroup = SearchResponseGroup.WithProducts }; // (!!!!- It needs to filter by CatalogIDs
                        x.TotalCount = _catalogSearchService.Search(productSearchCriteria).ProductsTotalCount;

                        var searchResponse = _catalogSearchService.Search(new SearchCriteria { WithHidden = true, Take = x.Take, Skip = x.Skip, ResponseGroup = SearchResponseGroup.WithProducts });

                        var products = _itemService.GetByIds(searchResponse.Products.Select(y => y.Id).ToArray(), ItemResponseGroup.ItemLarge);
                        foreach (var product in products)
                        {
                            ResetRedundantReferences(product);
                        }
                        x.Result = products.Cast<ExportableCatalogProduct>();
                    }
                    )
                }
            });

        }
        //Remove redundant references to reduce resulting JSON size. Copypasted from VirtoCommerce.CatalogModule.Web.ExportImport.CatalogExportImport
        private static void ResetRedundantReferences(object entity)
        {
            var product = entity as CatalogProduct;
            var category = entity as Category;
            var catalog = entity as Catalog;
            var asscociation = entity as ProductAssociation;
            var property = entity as Property;
            var propertyValue = entity as PropertyValue;

            if (propertyValue != null)
            {
                propertyValue.Property = null;
            }

            if (asscociation != null)
            {
                asscociation.AssociatedObject = null;
            }

            if (catalog != null)
            {
                catalog.Properties = null;
                foreach (var lang in catalog.Languages)
                {
                    lang.Catalog = null;
                }
            }

            if (category != null)
            {
                category.Catalog = null;
                category.Properties = null;
                category.Children = null;
                category.Parents = null;
                category.Outlines = null;
                if (category.PropertyValues != null)
                {
                    foreach (var propvalue in category.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
            }

            if (property != null)
            {
                property.Catalog = null;
                property.Category = null;
            }

            if (product != null)
            {
                product.Catalog = null;
                product.Category = null;
                product.Properties = null;
                product.MainProduct = null;
                product.Outlines = null;
                product.ReferencedAssociations = null;
                if (product.PropertyValues != null)
                {
                    foreach (var propvalue in product.PropertyValues)
                    {
                        ResetRedundantReferences(propvalue);
                    }
                }
                if (product.Associations != null)
                {
                    foreach (var association in product.Associations)
                    {
                        ResetRedundantReferences(association);
                    }
                }
                if (product.Variations != null)
                {
                    foreach (var variation in product.Variations)
                    {
                        ResetRedundantReferences(variation);
                    }
                }
            }
        }
    }
}
