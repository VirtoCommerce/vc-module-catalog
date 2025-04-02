angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.propertyGroups', ['$resource', function ($resource) {
        return $resource('api/catalog/propertygroups/:id', { id: '@Id' }, {
            save: { method: 'POST', url: 'api/catalog/propertygroups' },
            remove: { method: 'DELETE', url: 'api/catalog/propertygroups' },
            search: { method: 'POST', url: 'api/catalog/propertygroups/search' }
        });
}]);
