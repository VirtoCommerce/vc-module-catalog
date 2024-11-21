angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.configurationsApi', ['$resource', function ($resource) {

        return $resource('api/catalog/products/configurations/:id', { id: '@Id' },
            {
                getConfigurationByProduct: {
                    method: 'GET',
                    url: 'api/catalog/products/:productId/configurations',
                    params: { productId: '@productId' }
                },
                search: { method: 'POST', url: 'api/catalog/products/configurations/search' },
                update: { method: 'POST', url: 'api/catalog/products/configurations' }
            });

    }]);

