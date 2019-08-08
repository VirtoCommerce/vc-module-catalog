angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.search', ['$resource', function ($resource) {
    return $resource('api/catalog/search', null, {      
        searchProducts: { method: 'POST' },
    });
}]);
