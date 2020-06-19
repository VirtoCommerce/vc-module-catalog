angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.dynamicAssociations', ['$resource', function ($resource) {
        return $resource('api/catalog/products/dynamicAssociations/:id', null, {
            search: { url: 'api/catalog/products/dynamicAssociations/search', method: 'POST' },
            save: { url: 'api/catalog/products/dynamicAssociations/', method: 'POST', isArray: true },
            remove: { url: 'api/catalog/products/dynamicAssociations/', method: 'DELETE' },
            new: { url: 'api/catalog/products/dynamicAssociations/new', method: 'GET' },
            preview: { url: 'api/catalog/products/dynamicAssociations/preview', method: 'POST', isArray: true }
        });
    }]);
