angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.automaticLinkQueryApi', ['$resource', function ($resource) {

        return $resource('api/catalog/automatic-link-queries/:id', { id: '@Id' }, {
            update: { method: 'PUT' },
            search: { method: 'POST', url: 'api/catalog/automatic-link-queries/search' },
        });
    }]);
