angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.sortOrderings', ['$resource', function ($resource) {
    return $resource('', {}, {
        getOrderings: { method: 'GET', url: 'api/catalog/sort-orderings/store/:storeId', isArray: true },
        saveOrderings: { method: 'PUT', url: 'api/catalog/sort-orderings/store/:storeId' },
        getFields: { method: 'GET', url: 'api/catalog/sort-orderings/store/:storeId/fields', isArray: true }
    });
}]);
