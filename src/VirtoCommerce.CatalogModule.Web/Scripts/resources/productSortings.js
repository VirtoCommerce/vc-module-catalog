angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.productSortings', ['$resource', function ($resource) {
    return $resource('', {}, {
        getSortings: { method: 'GET', url: 'api/catalog/product-sortings/store/:storeId', isArray: true },
        saveSortings: { method: 'PUT', url: 'api/catalog/product-sortings/store/:storeId' },
        getFields: { method: 'GET', url: 'api/catalog/product-sortings/store/:storeId/fields', isArray: true }
    });
}]);
