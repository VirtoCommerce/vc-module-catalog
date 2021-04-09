angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.associations', ['$resource', function ($resource) {
        return $resource('api/catalog/products/associations/:productId', null, {
            get: { method: 'GET', isArray: true },
            update: { method: 'POST', url: 'api/catalog/products/associations' },
            delete: { method: 'DELETE', url: 'api/catalog/products/associations' },
            search: { method: 'POST', url: 'api/catalog/products/associations/search' }
        });
    }]);
