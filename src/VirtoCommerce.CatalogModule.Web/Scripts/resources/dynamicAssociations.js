angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.dynamicAssociations', ['$resource', function ($resource) {
        return $resource('api/marketing/associations/:id', null, {
            search: { url: 'api/marketing/associations/search', method: 'POST' }
        });
    }]);
