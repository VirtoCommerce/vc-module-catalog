angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.categories', ['$resource', function ($resource) {

        return $resource('api/catalog/categories/:id', { categoryId: '@Id' }, {
            newCategory: { method: 'GET', url: 'api/catalog/:catalogId/categories/newcategory', params: { catalogId: '@catalogId' } },
            update: { method: 'POST', url: 'api/catalog/categories/' },
            remove: { method: 'DELETE', url: 'api/catalog/categories' },
            plenty: { method: 'POST', url: 'api/catalog/categories/plenty', isArray: true },
            getByIds: { method: 'GET', url: 'api/catalog/categories', params: { ids: '@ids' }, isArray: true },
            updateAutomaticLinks: { method: 'POST', url: 'api/catalog/categories/:id/automatic-links', params: { id: '@id' } },
            deleteAutomaticLinks: { method: 'DELETE', url: 'api/catalog/categories/:id/automatic-links', params: { id: '@id' } },
        });

    }]);

