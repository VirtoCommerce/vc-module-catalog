angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.search', ['$resource', function ($resource) {
        return $resource('api/catalog/listentries', null, {
            searchProducts: { method: 'POST' },
        });
    }]);
