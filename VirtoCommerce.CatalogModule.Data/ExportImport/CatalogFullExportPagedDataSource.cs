using System.Collections.Generic;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Extensions;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportPagedDataSource : IPagedDataSource
    {
        private readonly CatalogExportPagedDataSourceFactory _catalogDataSourceFactory;
        private readonly CategoryExportPagedDataSourceFactory _categoryDataSourceFactory;
        private readonly PropertyDictionaryItemExportPagedDataSourceFactory _propertyDictionaryItemExportPagedDataSourceFactory;
        private readonly PropertyExportPagedDataSourceFactory _propertyExportPagedDataSourceFactory;
        //private readonly ICatalogSearchService _catalogSearchService;
        //private readonly ICategoryService _categoryService;
        //private readonly IItemService _itemService;
        //private readonly IPropertyService _propertyService;
        //private readonly IAssociationService _associationService;
        //private readonly IProperyDictionaryItemSearchService _propertyDictionarySearchService;
        //private readonly IProperyDictionaryItemService _propertyDictionaryService;

        private readonly CatalogFullExportDataQuery _dataQuery;
        private readonly IEnumerable<IPagedDataSource> _dataSources;

        public int CurrentPageNumber { get; protected set; }
        public int PageSize { get; set; } = 50;
        public int? Skip { get => _dataQuery.Skip; set => _dataQuery.Skip = value; }
        public int? Take { get => _dataQuery.Take; set => _dataQuery.Take = value; }
        public IEnumerable<IExportable> Items { get; protected set; }

        public CatalogFullExportPagedDataSource(CatalogExportPagedDataSourceFactory catalogDataSourceFactory,
            CategoryExportPagedDataSourceFactory categoryDataSourceFactory,
            PropertyDictionaryItemExportPagedDataSourceFactory propertyDictionaryItemExportPagedDataSourceFactory,
            PropertyExportPagedDataSourceFactory propertyExportPagedDataSourceFactory,
            //ICatalogSearchService catalogSearchService,
            //ICategoryService categoryService,
            //IItemService itemService,
            //IPropertyService propertyService,
            //IAssociationService associationService,
            //IProperyDictionaryItemSearchService propertyDictionarySearchService,
            //IProperyDictionaryItemService propertyDictionaryService,
            CatalogFullExportDataQuery dataQuery)
        {
            _catalogDataSourceFactory = catalogDataSourceFactory;
            _categoryDataSourceFactory = categoryDataSourceFactory;
            _propertyExportPagedDataSourceFactory = propertyExportPagedDataSourceFactory;
            _propertyDictionaryItemExportPagedDataSourceFactory = propertyDictionaryItemExportPagedDataSourceFactory;
            //_catalogSearchService = catalogSearchService;
            //_categoryService = categoryService;
            //_itemService = itemService;
            //_propertyService = propertyService;
            //_associationService = associationService;
            //_propertyDictionarySearchService = propertyDictionarySearchService;
            //_propertyDictionaryService = propertyDictionaryService;

            _dataQuery = dataQuery;
            _dataSources = CreateDataSources();
        }

        protected virtual IEnumerable<IPagedDataSource> CreateDataSources()
        {
            var catalogExportDataQuery = AbstractTypeFactory<CatalogExportDataQuery>.TryCreateInstance();
            catalogExportDataQuery.CatalogIds = _dataQuery.CatalogIds;

            var categoryExportDataQuery = AbstractTypeFactory<CategoryExportDataQuery>.TryCreateInstance();
            categoryExportDataQuery.CatalogIds = _dataQuery.CatalogIds;

            var propertyExportDataQuery = AbstractTypeFactory<PropertyExportDataQuery>.TryCreateInstance();
            propertyExportDataQuery.CatalogIds = _dataQuery.CatalogIds;

            var propertyDictionaryItemExportDataQuery = AbstractTypeFactory<PropertyDictionaryItemExportDataQuery>.TryCreateInstance();
            propertyDictionaryItemExportDataQuery.CatalogIds = _dataQuery.CatalogIds;

            //var productExportDataQuery = AbstractTypeFactory<ProductExportDataQuery>.TryCreateInstance();
            //productExportDataQuery.CatalogIds = _dataQuery.CatalogIds;
            //productExportDataQuery.SearchInVariations = true;
            //productExportDataQuery.ResponseGroup = (ItemResponseGroup.ItemLarge & ~ItemResponseGroup.Variations).ToString();

            return new IPagedDataSource[]
            {
                _propertyExportPagedDataSourceFactory.Create(propertyExportDataQuery),
                _propertyDictionaryItemExportPagedDataSourceFactory.Create(propertyDictionaryItemExportDataQuery),
                _catalogDataSourceFactory.Create(catalogExportDataQuery),
                _categoryDataSourceFactory.Create(categoryExportDataQuery),
                //_propertyDataSourceFactory.Create(propertyExportDataQuery),
                //_propertyDictionaryItemDataSourceFactory.Create(propertyDictionaryItemExportDataQuery),
                //_productDataSourceFactory.Create(productExportDataQuery),
            };
        }

        public bool Fetch()
        {
            var skip = Skip ?? CurrentPageNumber * PageSize;
            var take = Take ?? PageSize;

            Items = _dataSources.GetItems(skip, take);
            CurrentPageNumber++;

            return !Items.IsNullOrEmpty();
        }

        public int GetTotalCount() => _dataSources.GetTotalCount();

        //protected override void InitDataSourceStates()
        //{
        //    _exportDataSourceStates.AddRange(new ExportDataSourceState[]
        //    {
        //        // -- ExportProperties
        //        new ExportDataSourceState
        //        {
        //            FetchFunc = (x) => Task.Factory.StartNew( ()=>
        //            {
        //                var allproperties = _propertyService.GetAllProperties(); // (!!!!- It needs to filter by CatalogIDs
        //                var properties = allproperties.Skip(x.Skip).Take(x.Take); // (!!!!- Terrible, but _propertyService does not have paged read)
        //                //Load property dictionary values and reset some props to decrease size of the resulting json 
        //                foreach (var property in properties)
        //                {
        //                    property.ResetRedundantReferences();
        //                }
        //                x.TotalCount = allproperties.Count();
        //                x.Result = properties.Select(y =>ExportableProperty.FromModel(y));
        //            }
        //            )
        //        },

        //        //ExportPropertiesDictionaryItems
        //        new ExportDataSourceState
        //        {
        //            FetchFunc = (x) => Task.Factory.StartNew( ()=>
        //            {
        //                var criteria = new PropertyDictionaryItemSearchCriteria { Take = x.Take, Skip = x.Skip }; // (!!!!- It needs to filter by CatalogIDs
        //                var searchResponse = _propertyDictionarySearchService.Search(criteria);
        //                x.TotalCount = searchResponse.TotalCount;
        //                x.Result = searchResponse.Results.Select(y =>ExportablePropertyDictionaryItem.FromModel(y));
        //            }
        //            )
        //        },
        //        // ExportProducts
        //        new ExportDataSourceState
        //        {
        //            FetchFunc = (x) => Task.Factory.StartNew( ()=>
        //            {
        //                var productSearchCriteria = new SearchCriteria { WithHidden = true, Take = x.Take, Skip = x.Skip, ResponseGroup = SearchResponseGroup.WithProducts }; // (!!!!- It needs to filter by CatalogIDs
        //                x.TotalCount = _catalogSearchService.Search(productSearchCriteria).ProductsTotalCount;

        //                var searchResponse = _catalogSearchService.Search(new SearchCriteria { WithHidden = true, Take = x.Take, Skip = x.Skip, ResponseGroup = SearchResponseGroup.WithProducts });

        //                var products = _itemService.GetByIds(searchResponse.Products.Select(y => y.Id).ToArray(), ItemResponseGroup.ItemLarge);
        //                foreach (var product in products)
        //                {
        //                    product.ResetRedundantReferences();
        //                }
        //                x.Result = products.Select(y =>ExportableCatalogProduct.FromModel(y));
        //            }
        //            )
        //        }
        //    });
        //}
    }
}
